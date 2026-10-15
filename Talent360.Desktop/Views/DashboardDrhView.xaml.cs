using System.Windows;
using System.Windows.Controls;
using Talent360.Application.Enums;
using Talent360.Desktop.ViewModels;

namespace Talent360.Desktop.Views;

public partial class DashboardDrhView : UserControl
{
    public DashboardDrhView()
    {
        InitializeComponent();
        FilterSourceComboBox.SelectedIndex = 1;
        ApplyFilterFromCombo();
    }

    private void FilterSourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilterFromCombo();
        if (DataContext is DashboardDrhViewModel vm)
        {
            vm.Refresh();
        }
    }

    private void ApplyFilterFromCombo()
    {
        if (FilterSourceComboBox.SelectedItem is ComboBoxItem item)
        {
            CollaborateurSourceFilter.Current = item.Content?.ToString() switch
            {
                "Tout" => CollaborateurFiltreSource.Tout,
                "Démonstration uniquement" => CollaborateurFiltreSource.DemonstrationUniquement,
                _ => CollaborateurFiltreSource.DonneesReellesUniquement
            };
        }
    }
}
