using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using Talent360.Application.Interfaces;
using Talent360.Infrastructure;
using Talent360.Infrastructure.Import;

namespace Talent360.Desktop.Views;

public partial class TalentPassportView : UserControl
{
    public TalentPassportView()
    {
        InitializeComponent();
    }

    private void Rechercher_Click(object sender, RoutedEventArgs e)
    {
        var matricule = TxtMatricule.Text?.Trim();
        if (string.IsNullOrWhiteSpace(matricule))
        {
            LblMessage.Text = "Veuillez saisir un matricule.";
            LblMessage.Foreground = new SolidColorBrush(Color.FromRgb(146, 64, 14));
            PanelFiche.Visibility = Visibility.Collapsed;
            return;
        }

        var app = (App)System.Windows.Application.Current;
        using var scope = app.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICollaborateurRepository>();
        var calculEngine = scope.ServiceProvider.GetRequiredService<ICalculEngine>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var collab = repo.ObtenirTous()
            .FirstOrDefault(c => c.Matricule.Equals(matricule, StringComparison.OrdinalIgnoreCase));

        if (collab == null)
        {
            LblMessage.Text = $"Aucun collaborateur trouvé avec le matricule « {matricule} ».";
            LblMessage.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));
            PanelFiche.Visibility = Visibility.Collapsed;
            return;
        }

        LblMessage.Text = "";
        PanelFiche.Visibility = Visibility.Visible;

        // Identité
        LblNom.Text = $"{collab.Prenom} {collab.Nom}";
        LblPoste.Text = collab.Poste;
        LblDirection.Text = collab.Direction;
        LblGrade.Text = $"Grade : {collab.Grade}";
        LblStatut.Text = collab.TalentValide ? "Talent validé"
            : collab.TalentPropose ? "Talent proposé"
            : "—";

        // ── Scores recalculés dynamiquement via CalculEngine ──
        LblPerf.Text = collab.ScorePerformance.ToString("F1");
        LblPot.Text = collab.ScorePotentiel.ToString("F1");
        LblEngagement.Text = collab.Engagement > 0 ? collab.Engagement.ToString() : "—";
        LblMatching.Text = collab.Matching.HasValue ? collab.Matching.Value.ToString("F0") : "—";

        // 9-Box recalculé dynamiquement
        var box = calculEngine.CalculerNeufBox(collab.ScorePerformance, collab.ScorePotentiel);
        LblBox9.Text = $"9-Box : {box}";

        // Catégorie de performance
        var catPerf = calculEngine.DeterminerCategoriePerformance(collab.ScorePerformance);
        LblCatPerf.Text = $"Catégorie : {catPerf}";

        // Talent info
        LblVivier.Text = collab.Vivier ?? "—";
        LblPosteCible.Text = collab.PosteCible ?? "—";
        LblReadiness.Text = collab.Readiness ?? "—";
        LblReadiness.Foreground = collab.Readiness switch
        {
            "Ready Now" => new SolidColorBrush(Color.FromRgb(5, 150, 105)),
            "< 1 an" => new SolidColorBrush(Color.FromRgb(37, 99, 235)),
            "1-2 ans" => new SolidColorBrush(Color.FromRgb(217, 119, 6)),
            _ => new SolidColorBrush(Color.FromRgb(107, 114, 128))
        };

        // ── Vigilance RH ──
        var recalcul = new ScoreRecalculService(context, calculEngine);
        var (indice, eng, mouv, mob, dev, baisse, reco, form) =
            recalcul.CalculerFlagsVigilance(collab.Matricule);

        var niveauVig = calculEngine.DeterminerNiveauVigilance(indice);
        LblVigilance.Text = $"Vigilance : {indice}/100 ({niveauVig})";
        LblVigilance.Foreground = niveauVig switch
        {
            Application.Enums.NiveauVigilance.Elevee => new SolidColorBrush(Color.FromRgb(220, 38, 38)),
            Application.Enums.NiveauVigilance.Moderee => new SolidColorBrush(Color.FromRgb(217, 119, 6)),
            _ => new SolidColorBrush(Color.FromRgb(5, 150, 105))
        };
    }
}
