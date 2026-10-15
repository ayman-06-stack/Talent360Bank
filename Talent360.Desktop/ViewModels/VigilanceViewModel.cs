using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Talent360.Application.Enums;
using Talent360.Application.Interfaces;
using Talent360.Infrastructure;
using Talent360.Infrastructure.Import;

namespace Talent360.Desktop.ViewModels;

/// <summary>
/// Ligne d'affichage pour un collaborateur dans la vue Vigilance RH.
/// </summary>
public class VigilanceLigneVm
{
    public string Matricule { get; set; } = string.Empty;
    public string NomComplet { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public string Poste { get; set; } = string.Empty;
    public int IndiceVigilance { get; set; }
    public string NiveauVigilance { get; set; } = string.Empty;
    public Brush CouleurNiveau { get; set; } = Brushes.Gray;

    // Détail des flags
    public bool EngagementFaible { get; set; }
    public bool AucunMouvement { get; set; }
    public bool MobiliteNonTraitee { get; set; }
    public bool AucunDevRecent { get; set; }
    public bool BaissePerformance { get; set; }
    public bool FaibleReconnaissance { get; set; }
    public bool FormationNonRealisee { get; set; }

    public string FacteursTexte { get; set; } = string.Empty;
}

public partial class VigilanceViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<VigilanceLigneVm> _collaborateurs = new();
    [ObservableProperty] private int _totalCollaborateurs;
    [ObservableProperty] private int _nombreElevee;
    [ObservableProperty] private int _nombreModeree;
    [ObservableProperty] private int _nombreFaible;
    [ObservableProperty] private bool _aDonnees;
    [ObservableProperty] private string _messageVide =
        "Aucune donnee de vigilance disponible. Importez les fichiers Carriere, Formation, Mobilite et Reconnaissance pour alimenter cet ecran.";
    [ObservableProperty] private string _messageErreur = string.Empty;

    public VigilanceViewModel()
    {
        try
        {
            Charger();
        }
        catch (Exception ex)
        {
            ADonnees = false;
            MessageErreur = $"Erreur lors du chargement : {ex.Message}";
        }
    }

    private void Charger()
    {
        var app = (App)System.Windows.Application.Current;
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var calculEngine = scope.ServiceProvider.GetRequiredService<ICalculEngine>();
        var recalcul = new ScoreRecalculService(context, calculEngine);

        var collabs = Talent360.Application.Enums.CollaborateurSourceFilter
            .Apply(context.Collaborateurs.AsQueryable()).ToList();
        if (!collabs.Any())
        {
            ADonnees = false;
            return;
        }

        var lignes = new System.Collections.Generic.List<VigilanceLigneVm>();

        foreach (var c in collabs)
        {
            try
            {
                var (indice, eng, mouv, mob, dev, baisse, reco, form) =
                    recalcul.CalculerFlagsVigilance(c.Matricule);

                var niveau = calculEngine.DeterminerNiveauVigilance(indice);

                var facteurs = new System.Collections.Generic.List<string>();
                if (eng) facteurs.Add("Engagement faible");
                if (mouv) facteurs.Add("Aucun mouvement");
                if (mob) facteurs.Add("Mobilite non traitee");
                if (dev) facteurs.Add("Aucun developpement recent");
                if (baisse) facteurs.Add("Baisse performance");
                if (reco) facteurs.Add("Faible reconnaissance");
                if (form) facteurs.Add("Formation non realisee");

                lignes.Add(new VigilanceLigneVm
                {
                    Matricule = c.Matricule,
                    NomComplet = $"{c.Prenom} {c.Nom}",
                    Direction = c.Direction,
                    Poste = c.Poste,
                    IndiceVigilance = indice,
                    NiveauVigilance = niveau switch
                    {
                        NiveauVigilance.Elevee => "Elevee",
                        NiveauVigilance.Moderee => "Moderee",
                        _ => "Faible"
                    },
                    CouleurNiveau = niveau switch
                    {
                        NiveauVigilance.Elevee => new SolidColorBrush(Color.FromRgb(220, 38, 38)),
                        NiveauVigilance.Moderee => new SolidColorBrush(Color.FromRgb(217, 119, 6)),
                        _ => new SolidColorBrush(Color.FromRgb(5, 150, 105))
                    },
                    EngagementFaible = eng,
                    AucunMouvement = mouv,
                    MobiliteNonTraitee = mob,
                    AucunDevRecent = dev,
                    BaissePerformance = baisse,
                    FaibleReconnaissance = reco,
                    FormationNonRealisee = form,
                    FacteursTexte = facteurs.Any() ? string.Join(" · ", facteurs) : "Aucun facteur identifie"
                });
            }
            catch
            {
                // Ignorer les erreurs par collaborateur pour ne pas bloquer la page entière
            }
        }

        // Trier par indice décroissant (les plus urgents en haut)
        var triees = lignes.OrderByDescending(l => l.IndiceVigilance).ToList();

        TotalCollaborateurs = triees.Count;
        NombreElevee = triees.Count(l => l.NiveauVigilance == "Elevee");
        NombreModeree = triees.Count(l => l.NiveauVigilance == "Moderee");
        NombreFaible = triees.Count(l => l.NiveauVigilance == "Faible");

        ADonnees = triees.Count > 0;
        Collaborateurs = new ObservableCollection<VigilanceLigneVm>(triees);
    }
}
