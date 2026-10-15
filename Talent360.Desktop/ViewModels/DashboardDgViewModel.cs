using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Talent360.Application.Services;

namespace Talent360.Desktop.ViewModels;

public partial class DashboardDgViewModel : ObservableObject
{
    [ObservableProperty] private double _tauxCouverture;
    [ObservableProperty] private int _postesCritiquesCouverts;
    [ObservableProperty] private int _postesCritiquesTotaux;
    [ObservableProperty] private int _nombreTalentsValides;
    [ObservableProperty] private int _postesCritiquesSansReleve;
    [ObservableProperty] private double _engagementMoyen;
    [ObservableProperty] private string _dateRapport = string.Empty;

    public DashboardDgViewModel()
    {
        var app = (App)System.Windows.Application.Current;
        using var scope = app.Services.CreateScope();

        var kpiService = scope.ServiceProvider.GetRequiredService<KpiService>();
        var kpi = kpiService.CalculerKpi();

        TauxCouverture = kpi.TauxCouverture;
        PostesCritiquesCouverts = kpi.PostesCritiquesCouverts;
        PostesCritiquesTotaux = kpi.PostesCritiquesTotaux;
        NombreTalentsValides = kpi.NombreTalentsValides;
        PostesCritiquesSansReleve = kpi.PostesCritiquesSansReleve;
        EngagementMoyen = kpi.EngagementMoyen;
        DateRapport = $"Rapport généré le {DateTime.Now:dd/MM/yyyy à HH:mm}";
    }
}
