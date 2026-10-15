using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Talent360.Application.Enums;
using Talent360.Infrastructure;

namespace Talent360.Desktop.ViewModels;

public partial class CampagneViewModel : ObservableObject
{
    [ObservableProperty] private int _nbCollaborateurs;
    [ObservableProperty] private int _nbAutoEvaluations;
    [ObservableProperty] private int _nbEvalManager;
    [ObservableProperty] private int _nbEngagement;
    [ObservableProperty] private int _nbScoresCalcules;

    [ObservableProperty] private double _pctAutoEval;
    [ObservableProperty] private double _pctEvalManager;
    [ObservableProperty] private double _pctEngagement;
    [ObservableProperty] private double _pctScores;

    public CampagneViewModel()
    {
        Charger();
    }

    private void Charger()
    {
        var app = (App)System.Windows.Application.Current;
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        NbCollaborateurs = CollaborateurSourceFilter.Apply(context.Collaborateurs.AsQueryable()).Count();

        // Matricules distincts avec auto-évaluation
        NbAutoEvaluations = context.ImportAutoEvaluations
            .Select(a => a.Matricule).Distinct().Count();

        NbEvalManager = context.ImportEvaluationManagers
            .Select(e => e.Matricule).Distinct().Count();

        NbEngagement = context.ImportEngagements
            .Select(e => e.Matricule).Distinct().Count();

        // Collaborateurs avec ScorePerformance > 0 (= score calculé)
        NbScoresCalcules = CollaborateurSourceFilter.Apply(context.Collaborateurs.AsQueryable())
            .Count(c => c.ScorePerformance > 0);

        // Pourcentages
        if (NbCollaborateurs > 0)
        {
            PctAutoEval = System.Math.Round((double)NbAutoEvaluations / NbCollaborateurs * 100, 1);
            PctEvalManager = System.Math.Round((double)NbEvalManager / NbCollaborateurs * 100, 1);
            PctEngagement = System.Math.Round((double)NbEngagement / NbCollaborateurs * 100, 1);
            PctScores = System.Math.Round((double)NbScoresCalcules / NbCollaborateurs * 100, 1);
        }
    }
}
