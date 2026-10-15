using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Talent360.Infrastructure;

namespace Talent360.Desktop.ViewModels;

// ──────────────────────────────────────────────────────────
// Sous-VM : une carte résumée pour chaque dimension
// ──────────────────────────────────────────────────────────
public class DimensionEngagementVm
{
    public string Nom { get; set; } = string.Empty;
    public string Icone { get; set; } = string.Empty;
    public double Score { get; set; }
    public string ScoreTexte => $"{Score:F1} %";
    public bool EstSous70 => Score < 70;

    // Couleurs calculées selon le seuil de 70 %
    public Brush CouleurFond => EstSous70
        ? new SolidColorBrush(Color.FromRgb(254, 226, 226))   // rouge pâle
        : new SolidColorBrush(Color.FromRgb(220, 252, 231));   // vert pâle

    public Brush CouleurBordure => EstSous70
        ? new SolidColorBrush(Color.FromRgb(239, 68, 68))      // rouge
        : new SolidColorBrush(Color.FromRgb(34, 197, 94));     // vert

    public Brush CouleurTexte => EstSous70
        ? new SolidColorBrush(Color.FromRgb(185, 28, 28))
        : new SolidColorBrush(Color.FromRgb(21, 128, 61));

    public string StatutTexte => EstSous70 ? "Sous le seuil" : "Conforme";
}

// ──────────────────────────────────────────────────────────
// Sous-VM : une ligne du tableau par Direction
// ──────────────────────────────────────────────────────────
public class LigneEngagementDirectionVm
{
    public string Direction { get; set; } = string.Empty;
    public int NombreReponses { get; set; }

    public double Management { get; set; }
    public double Reconnaissance { get; set; }
    public double Formation { get; set; }
    public double EvolutionPro { get; set; }
    public double ConditionsTravail { get; set; }
    public double Communication { get; set; }
    public double Moyenne { get; set; }

    // Brushes pour chaque cellule dimension (rouge si < 70 %)
    public Brush CouleurManagement => BrushPour(Management);
    public Brush CouleurReconnaissance => BrushPour(Reconnaissance);
    public Brush CouleurFormation => BrushPour(Formation);
    public Brush CouleurEvolutionPro => BrushPour(EvolutionPro);
    public Brush CouleurConditionsTravail => BrushPour(ConditionsTravail);
    public Brush CouleurCommunication => BrushPour(Communication);
    public Brush CouleurMoyenne => BrushPour(Moyenne);

    private static Brush BrushPour(double score) => score < 70
        ? new SolidColorBrush(Color.FromRgb(220, 38, 38))      // rouge
        : new SolidColorBrush(Color.FromRgb(5, 150, 105));     // vert
}

// ──────────────────────────────────────────────────────────
// ViewModel principal
// ──────────────────────────────────────────────────────────
public partial class EngagementViewModel : ObservableObject
{
    // Données / état
    [ObservableProperty] private bool _aDonnees;
    [ObservableProperty] private string _messageVide =
        "Aucune donnée d'engagement importée — importez le fichier engagement.csv via l'écran Import de données.";

    // Filtre Direction
    [ObservableProperty] private ObservableCollection<string> _directionsDisponibles = new();
    [ObservableProperty] private string _directionSelectionnee = "Toutes";

    // KPI globaux
    [ObservableProperty] private int _nombreReponsesTotal;
    [ObservableProperty] private double _engagementMoyenGlobal;
    [ObservableProperty] private int _dimensionsSous70;

    // Cartes dimensions
    [ObservableProperty] private ObservableCollection<DimensionEngagementVm> _dimensions = new();

    // Tableau détaillé par Direction
    [ObservableProperty] private ObservableCollection<LigneEngagementDirectionVm> _lignesParDirection = new();

    // Données brutes en mémoire (évite de re-lire la DB à chaque filtre)
    private List<EngagementAvecDirection> _donneesCompletes = new();

    public EngagementViewModel()
    {
        ChargerDepuisDb();
        Recalculer();
    }

    // Quand la direction sélectionnée change → recalculer
    partial void OnDirectionSelectionneeChanged(string value) => Recalculer();

    // ──────────── Chargement DB ────────────
    private void ChargerDepuisDb()
    {
        var app = (App)System.Windows.Application.Current;
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var engagements = context.ImportEngagements.ToList();
        if (!engagements.Any())
        {
            ADonnees = false;
            return;
        }

        // Récupérer les collaborateurs pour le mapping Matricule → Direction
        var collabDict = Talent360.Application.Enums.CollaborateurSourceFilter
            .Apply(context.Collaborateurs.AsQueryable())
            .ToDictionary(c => c.Matricule, c => c.Direction, StringComparer.OrdinalIgnoreCase);

        _donneesCompletes = engagements.Select(e => new EngagementAvecDirection
        {
            Matricule = e.Matricule,
            Direction = collabDict.TryGetValue(e.Matricule, out var dir) ? dir : "(Inconnue)",
            Management = e.Management,
            Reconnaissance = e.Reconnaissance,
            Formation = e.Formation,
            EvolutionPro = e.EvolutionPro,
            ConditionsTravail = e.ConditionsTravail,
            Communication = e.Communication
        }).ToList();

        // Populer les directions
        var dirs = _donneesCompletes
            .Select(d => d.Direction)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(d => d)
            .ToList();

        DirectionsDisponibles = new ObservableCollection<string>(new[] { "Toutes" }.Concat(dirs));
        DirectionSelectionnee = "Toutes";

        ADonnees = true;
    }

    // ──────────── Recalcul sur filtre ────────────
    private void Recalculer()
    {
        if (!_donneesCompletes.Any()) return;

        // Filtrer
        var filtrees = DirectionSelectionnee == "Toutes"
            ? _donneesCompletes
            : _donneesCompletes.Where(d =>
                d.Direction.Equals(DirectionSelectionnee, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!filtrees.Any())
        {
            Dimensions = new ObservableCollection<DimensionEngagementVm>();
            LignesParDirection = new ObservableCollection<LigneEngagementDirectionVm>();
            NombreReponsesTotal = 0;
            EngagementMoyenGlobal = 0;
            DimensionsSous70 = 0;
            return;
        }

        NombreReponsesTotal = filtrees.Count;

        // Moyennes globales par dimension
        var dims = new List<DimensionEngagementVm>
        {
            new() { Nom = "Management",          Icone = "👔", Score = Math.Round(filtrees.Average(f => f.Management), 1) },
            new() { Nom = "Reconnaissance",      Icone = "🏆", Score = Math.Round(filtrees.Average(f => f.Reconnaissance), 1) },
            new() { Nom = "Formation",           Icone = "📚", Score = Math.Round(filtrees.Average(f => f.Formation), 1) },
            new() { Nom = "Évolution Pro",       Icone = "🚀", Score = Math.Round(filtrees.Average(f => f.EvolutionPro), 1) },
            new() { Nom = "Conditions Travail",  Icone = "🏢", Score = Math.Round(filtrees.Average(f => f.ConditionsTravail), 1) },
            new() { Nom = "Communication",       Icone = "💬", Score = Math.Round(filtrees.Average(f => f.Communication), 1) },
        };

        Dimensions = new ObservableCollection<DimensionEngagementVm>(dims);
        EngagementMoyenGlobal = Math.Round(dims.Average(d => d.Score), 1);
        DimensionsSous70 = dims.Count(d => d.EstSous70);

        // Tableau par Direction
        var lignes = filtrees
            .GroupBy(f => f.Direction, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                double mgt  = Math.Round(g.Average(x => x.Management), 1);
                double rec  = Math.Round(g.Average(x => x.Reconnaissance), 1);
                double frm  = Math.Round(g.Average(x => x.Formation), 1);
                double evo  = Math.Round(g.Average(x => x.EvolutionPro), 1);
                double cdt  = Math.Round(g.Average(x => x.ConditionsTravail), 1);
                double com  = Math.Round(g.Average(x => x.Communication), 1);
                double moy  = Math.Round(new[] { mgt, rec, frm, evo, cdt, com }.Average(), 1);

                return new LigneEngagementDirectionVm
                {
                    Direction = g.Key,
                    NombreReponses = g.Count(),
                    Management = mgt,
                    Reconnaissance = rec,
                    Formation = frm,
                    EvolutionPro = evo,
                    ConditionsTravail = cdt,
                    Communication = com,
                    Moyenne = moy
                };
            })
            .OrderBy(l => l.Moyenne)
            .ToList();

        LignesParDirection = new ObservableCollection<LigneEngagementDirectionVm>(lignes);
    }

    // ──────────── DTO interne ────────────
    private class EngagementAvecDirection
    {
        public string Matricule { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public double Management { get; set; }
        public double Reconnaissance { get; set; }
        public double Formation { get; set; }
        public double EvolutionPro { get; set; }
        public double ConditionsTravail { get; set; }
        public double Communication { get; set; }
    }
}
