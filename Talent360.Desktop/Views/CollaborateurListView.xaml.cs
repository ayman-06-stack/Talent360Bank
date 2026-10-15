using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Talent360.Application.Enums;
using Talent360.Application.Interfaces;
using Talent360.Desktop.ViewModels;

namespace Talent360.Desktop.Views;

public partial class CollaborateurListView : UserControl
{
    public CollaborateurListView()
    {
        InitializeComponent();
        SourceFilterComboBox.SelectedIndex = 1;
        ApplyFilterFromCombo();
    }

    private void SourceFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilterFromCombo();
        if (DataContext is CollaborateurListViewModel viewModel)
        {
            viewModel.Refresh();
        }
    }

    private void ApplyFilterFromCombo()
    {
        if (SourceFilterComboBox.SelectedItem is ComboBoxItem item)
        {
            CollaborateurSourceFilter.Current = item.Content?.ToString() switch
            {
                "Tout" => CollaborateurFiltreSource.Tout,
                "Démonstration uniquement" => CollaborateurFiltreSource.DemonstrationUniquement,
                _ => CollaborateurFiltreSource.DonneesReellesUniquement
            };
        }
    }

    private void ExporterExcel_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Fichiers Excel (*.xlsx)|*.xlsx",
            FileName = $"Talent360_Export_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
            Title = "Exporter les collaborateurs"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var app = (App)System.Windows.Application.Current;
                using var scope = app.Services.CreateScope();
                var exportService = scope.ServiceProvider.GetRequiredService<IExportService>();
                exportService.ExporterCollaborateurs(dialog.FileName);

                System.Windows.MessageBox.Show(
                    $"Export réussi !\n\n{dialog.FileName}",
                    "Talent360Bank — Export",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Erreur lors de l'export :\n\n{ex.Message}",
                    "Talent360Bank — Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
