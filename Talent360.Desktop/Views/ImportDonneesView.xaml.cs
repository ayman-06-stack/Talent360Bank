using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Talent360.Application.Interfaces;
using Talent360.Infrastructure;
using Talent360.Infrastructure.Import;

namespace Talent360.Desktop.Views;

public partial class ImportDonneesView : UserControl
{
    public ImportDonneesView()
    {
        InitializeComponent();
    }

    // ── Filtre OpenFileDialog commun ──
    private const string FileFilter = "Fichiers de données (*.csv;*.xlsx)|*.csv;*.xlsx|CSV (*.csv)|*.csv|Excel (*.xlsx)|*.xlsx";

    private string? OuvrirFichier(string titre)
    {
        var dlg = new OpenFileDialog
        {
            Title = titre,
            Filter = FileFilter,
            Multiselect = false
        };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    private bool EstExcel(string path) =>
        Path.GetExtension(path).Equals(".xlsx", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Flux complet : preview → confirmation → backup → import → recalcul.
    /// </summary>
    private void ExecuterImportAvecPreview(string titre, string nomSource, TextBlock lblResult,
        Func<CsvImportService, string, int> importCsv,
        Func<ExcelImportService, string, int> importExcel,
        string labelSucces, bool avecRecalcul,
        string colonneMatricule = "Matricule")
    {
        var path = OuvrirFichier(titre);
        if (path == null) return;

        try
        {
            var app = (App)System.Windows.Application.Current;
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // ── ÉTAPE 1 : Prévisualisation ──
            var previewService = new ImportPreviewService(context);
            var preview = EstExcel(path)
                ? previewService.PrevisualiserExcel(path, nomSource, colonneMatricule)
                : previewService.PrevisualiserCsv(path, nomSource, colonneMatricule);

            // ── ÉTAPE 2 : Fenêtre de confirmation ──
            var previewWindow = new ImportPreviewWindow(preview, path)
            {
                Owner = Window.GetWindow(this)
            };

            if (previewWindow.ShowDialog() != true || !previewWindow.Confirme)
            {
                lblResult.Text = "Import annulé par l'utilisateur.";
                lblResult.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(107, 114, 128));
                return;
            }

            // ── ÉTAPE 3 : Sauvegarde de la base ──
            var backupPath = DatabaseBackupService.CreerSauvegarde();

            // ── ÉTAPE 4 : Import effectif ──
            int count = EstExcel(path)
                ? importExcel(new ExcelImportService(context), path)
                : importCsv(new CsvImportService(context), path);

            // ── ÉTAPE 5 : Recalcul des scores (si applicable) ──
            string recalculMsg = "";
            if (avecRecalcul)
            {
                var calcul = scope.ServiceProvider.GetRequiredService<ICalculEngine>();
                var recalcul = new ScoreRecalculService(context, calcul);
                int updated = recalcul.RecalculerTout();
                recalculMsg = $" — {updated} scores recalculés";
            }

            // ── Résultat ──
            lblResult.Text = $"{count} {labelSucces}{recalculMsg}\n" +
                             $"Sauvegarde : {backupPath}";
            lblResult.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(5, 150, 105));
        }
        catch (Exception ex)
        {
            lblResult.Text = $"Erreur : {ex.Message}";
            lblResult.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(220, 38, 38));
        }
    }

    // ═══════════════════════════════════════════
    //  HANDLERS — Sources principales (avec recalcul)
    // ═══════════════════════════════════════════

    private void ImportSirh_Click(object sender, RoutedEventArgs e) =>
        ExecuterImportAvecPreview("01 — Fichier SIRH", "01 — SIRH (collaborateurs)",
            LblResultSirh,
            (csv, p) => csv.ImporterSirh(p),
            (xls, p) => xls.ImporterSirh(p),
            "collaborateurs importés", avecRecalcul: true);

    private void ImportAutoEval_Click(object sender, RoutedEventArgs e) =>
        ExecuterImportAvecPreview("02 — Auto-évaluations", "02 — Auto-évaluations",
            LblResultAutoEval,
            (csv, p) => csv.ImporterAutoEvaluation(p),
            (xls, p) => xls.ImporterAutoEvaluation(p),
            "auto-évaluations importées", avecRecalcul: true);

    private void ImportEvalManager_Click(object sender, RoutedEventArgs e) =>
        ExecuterImportAvecPreview("03 — Évaluations Manager", "03 — Évaluations Manager",
            LblResultEvalMgr,
            (csv, p) => csv.ImporterEvaluationManager(p),
            (xls, p) => xls.ImporterEvaluationManager(p),
            "évaluations manager importées", avecRecalcul: true);

    private void ImportPotentiel_Click(object sender, RoutedEventArgs e) =>
        ExecuterImportAvecPreview("04 — Potentiel", "04 — Potentiel (7 axes)",
            LblResultPotentiel,
            (csv, p) => csv.ImporterPotentiel(p),
            (xls, p) => xls.ImporterPotentiel(p),
            "évaluations potentiel importées", avecRecalcul: true);

    private void ImportEngagement_Click(object sender, RoutedEventArgs e) =>
        ExecuterImportAvecPreview("11 — Engagement", "11 — Engagement",
            LblResultEngagement,
            (csv, p) => csv.ImporterEngagement(p),
            (xls, p) => xls.ImporterEngagement(p),
            "réponses engagement importées", avecRecalcul: true);

    // ═══════════════════════════════════════════
    //  HANDLERS — Sources complementaires (avec recalcul)
    // ═══════════════════════════════════════════

    private void ImportFormation_Click(object sender, RoutedEventArgs e) =>
        ExecuterImportAvecPreview("05 — Formation", "05 — Formation",
            LblResultFormation,
            (csv, p) => csv.ImporterFormation(p),
            (xls, p) => xls.ImporterFormation(p),
            "formations importees", avecRecalcul: true);

    private void ImportMobilite_Click(object sender, RoutedEventArgs e) =>
        ExecuterImportAvecPreview("06 — Mobilite", "06 — Mobilite",
            LblResultMobilite,
            (csv, p) => csv.ImporterMobilite(p),
            (xls, p) => xls.ImporterMobilite(p),
            "demandes de mobilite importees", avecRecalcul: true);

    private void ImportSuccession_Click(object sender, RoutedEventArgs e) =>
        ExecuterImportAvecPreview("07 — Succession", "07 — Succession",
            LblResultSuccession,
            (csv, p) => csv.ImporterSuccession(p),
            (xls, p) => xls.ImporterSuccession(p),
            "plans de succession importes", avecRecalcul: true,
            colonneMatricule: "MatriculeSuccesseur");

    private void ImportCarriere_Click(object sender, RoutedEventArgs e) =>
        ExecuterImportAvecPreview("08 — Carriere", "08 — Carriere (mouvements)",
            LblResultCarriere,
            (csv, p) => csv.ImporterCarriere(p),
            (xls, p) => xls.ImporterCarriere(p),
            "mouvements de carriere importes", avecRecalcul: true);

    private void ImportReconnaissance_Click(object sender, RoutedEventArgs e) =>
        ExecuterImportAvecPreview("09 — Reconnaissance", "09 — Reconnaissance",
            LblResultReconnaissance,
            (csv, p) => csv.ImporterReconnaissance(p),
            (xls, p) => xls.ImporterReconnaissance(p),
            "evenements de reconnaissance importes", avecRecalcul: true);

    private void ImportLeadership_Click(object sender, RoutedEventArgs e) =>
        ExecuterImportAvecPreview("10 — Leadership 360", "10 — Leadership 360",
            LblResultLeadership,
            (csv, p) => csv.ImporterLeadership(p),
            (xls, p) => xls.ImporterLeadership(p),
            "scores leadership importes", avecRecalcul: true);

    private void ImportVigilance_Click(object sender, RoutedEventArgs e) =>
        ExecuterImportAvecPreview("12 — Vigilance RH", "12 — Vigilance RH",
            LblResultVigilance,
            (csv, p) => csv.ImporterVigilance(p),
            (xls, p) => xls.ImporterVigilance(p),
            "donnees vigilance importees", avecRecalcul: true);
}
