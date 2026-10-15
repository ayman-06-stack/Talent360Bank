using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.DependencyInjection;
using Talent360.Application.Enums;
using Talent360.Application.Interfaces;
using Talent360.Application.Models;
using Talent360.Application.Services;

namespace Talent360.Desktop.ViewModels;

public partial class DashboardDrhViewModel : ObservableObject
{
    // KPI
    [ObservableProperty] private int _populationTotale;
    [ObservableProperty] private int _nombreTalentsValides;
    [ObservableProperty] private int _nombreHautsPotentiels;
    [ObservableProperty] private int _nombreViviersActifs;
    [ObservableProperty] private int _postesCritiquesTotaux;
    [ObservableProperty] private int _postesCritiquesCouverts;
    [ObservableProperty] private int _postesCritiquesSansReleve;
    [ObservableProperty] private double _tauxCouverture;
    [ObservableProperty] private int _nombreReadyNow;
    [ObservableProperty] private double _engagementMoyen;

    // Graphique
    [ObservableProperty] private ISeries[] _neufBoxSeries = Array.Empty<ISeries>();

    // Alertes
    [ObservableProperty] private ObservableCollection<AlerteViewModel> _alertes = new();

    public DashboardDrhViewModel()
    {
        Refresh();
    }

    public void Refresh()
    {
        var app = (App)System.Windows.Application.Current;
        using var scope = app.Services.CreateScope();

        var kpiService = scope.ServiceProvider.GetRequiredService<KpiService>();
        var alerteService = scope.ServiceProvider.GetRequiredService<AlerteService>();
        var collaborateurRepo = scope.ServiceProvider.GetRequiredService<ICollaborateurRepository>();
        var calculEngine = scope.ServiceProvider.GetRequiredService<ICalculEngine>();

        var kpi = kpiService.CalculerKpi();
        PopulationTotale = kpi.PopulationTotale;
        NombreTalentsValides = kpi.NombreTalentsValides;
        NombreHautsPotentiels = kpi.NombreHautsPotentiels;
        NombreViviersActifs = kpi.NombreViviersActifs;
        PostesCritiquesTotaux = kpi.PostesCritiquesTotaux;
        PostesCritiquesCouverts = kpi.PostesCritiquesCouverts;
        PostesCritiquesSansReleve = kpi.PostesCritiquesSansReleve;
        TauxCouverture = kpi.TauxCouverture;
        NombreReadyNow = kpi.NombreReadyNow;
        EngagementMoyen = kpi.EngagementMoyen;

        var collaborateurs = collaborateurRepo.ObtenirTous();
        var groupes = collaborateurs
            .GroupBy(c => calculEngine.CalculerNeufBox(c.ScorePerformance, c.ScorePotentiel))
            .ToDictionary(g => g.Key, g => g.Count());

        var noms9Box = new Dictionary<CategorieNeufBox, string>
        {
            [CategorieNeufBox.TalentCle] = "Talent clé",
            [CategorieNeufBox.HautPotentiel] = "Haut potentiel",
            [CategorieNeufBox.PotentielARevelier] = "Potentiel à révéler",
            [CategorieNeufBox.PilierSolide] = "Pilier solide",
            [CategorieNeufBox.Confirme] = "Confirmé",
            [CategorieNeufBox.ADevelopper] = "À développer",
            [CategorieNeufBox.Expert] = "Expert",
            [CategorieNeufBox.AAccompagner] = "À accompagner",
            [CategorieNeufBox.ASurveiller] = "À surveiller"
        };

        NeufBoxSeries = Enum.GetValues<CategorieNeufBox>()
            .Where(cat => groupes.ContainsKey(cat) && groupes[cat] > 0)
            .Select(cat => new PieSeries<int>
            {
                Values = new[] { groupes[cat] },
                Name = $"{noms9Box[cat]} ({groupes[cat]})"
            })
            .ToArray();

        var alertesRH = alerteService.GenererAlertes();
        Alertes = new ObservableCollection<AlerteViewModel>(
            alertesRH.Select(a => new AlerteViewModel
            {
                Message = a.Message,
                Niveau = a.Niveau,
                NiveauIcon = a.Niveau switch
                {
                    NiveauAlerte.Critique => "●",
                    NiveauAlerte.Attention => "▲",
                    _ => "ℹ"
                },
                Background = a.Niveau switch
                {
                    NiveauAlerte.Critique => new SolidColorBrush(Color.FromRgb(254, 226, 226)),
                    NiveauAlerte.Attention => new SolidColorBrush(Color.FromRgb(254, 243, 199)),
                    _ => new SolidColorBrush(Color.FromRgb(219, 234, 254))
                }
            }));
    }
}

public class AlerteViewModel
{
    public string Message { get; set; } = string.Empty;
    public NiveauAlerte Niveau { get; set; }
    public string NiveauIcon { get; set; } = string.Empty;
    public Brush Background { get; set; } = Brushes.Transparent;
}
