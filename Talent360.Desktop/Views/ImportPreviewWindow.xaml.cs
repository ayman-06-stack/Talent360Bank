using System.Data;
using System.Windows;
using Talent360.Infrastructure.Import;

namespace Talent360.Desktop.Views;

public partial class ImportPreviewWindow : Window
{
    public bool Confirme { get; private set; }

    public ImportPreviewWindow(ImportPreviewData preview, string cheminFichier)
    {
        InitializeComponent();

        LblTitre.Text = $"Import : {preview.NomSource}";
        LblFichier.Text = $"Fichier : {cheminFichier}";
        LblTotalLignes.Text = preview.TotalLignes.ToString();
        LblNouveaux.Text = preview.NouveauxMatricules.ToString();
        LblMisAJour.Text = preview.MatriculesMisAJour.ToString();

        // Construire un DataTable pour le DataGrid d'aperçu
        var dt = new DataTable();
        foreach (var col in preview.Colonnes)
            dt.Columns.Add(col);

        foreach (var row in preview.AperçuLignes)
        {
            var dr = dt.NewRow();
            for (int i = 0; i < Math.Min(row.Length, preview.Colonnes.Count); i++)
                dr[i] = row[i];
            dt.Rows.Add(dr);
        }

        GridApercu.ItemsSource = dt.DefaultView;
    }

    private void Confirmer_Click(object sender, RoutedEventArgs e)
    {
        Confirme = true;
        DialogResult = true;
        Close();
    }

    private void Annuler_Click(object sender, RoutedEventArgs e)
    {
        Confirme = false;
        DialogResult = false;
        Close();
    }
}
