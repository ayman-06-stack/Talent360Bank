using Talent360.Application.Enums;
using Talent360.Application.Interfaces;
using Talent360.Application.Models;

namespace Talent360.Application.Services;

/// <summary>
/// Service de calcul des KPI agrégés pour les dashboards.
/// </summary>
public class KpiService
{
    private readonly ICollaborateurRepository _collaborateurRepo;
    private readonly IPosteCritiqueRepository _posteCritiqueRepo;
    private readonly ICalculEngine _calculEngine;

    public KpiService(
        ICollaborateurRepository collaborateurRepo,
        IPosteCritiqueRepository posteCritiqueRepo,
        ICalculEngine calculEngine)
    {
        _collaborateurRepo = collaborateurRepo;
        _posteCritiqueRepo = posteCritiqueRepo;
        _calculEngine = calculEngine;
    }

    public KpiResult CalculerKpi()
    {
        var collaborateurs = _collaborateurRepo.ObtenirTous();
        var postesCritiques = _posteCritiqueRepo.ObtenirTous();

        // Population
        int populationTotale = collaborateurs.Count;
        int talentsValides = collaborateurs.Count(c => c.TalentValide);
        int hautsPotentiels = collaborateurs.Count(c =>
            _calculEngine.CalculerNeufBox(c.ScorePerformance, c.ScorePotentiel) == CategorieNeufBox.HautPotentiel);

        // Viviers actifs (effectif > 0)
        var viviersActifs = collaborateurs
            .Where(c => !string.IsNullOrEmpty(c.Vivier))
            .Select(c => c.Vivier)
            .Distinct()
            .Count();

        // Postes critiques
        int pcTotal = postesCritiques.Count;
        int pcCouverts = 0;
        int pcSansReleve = 0;
        int gaps = 0;

        foreach (var pc in postesCritiques)
        {
            // Successeurs = collaborateurs de même direction avec matching calculé
            int nbSuccesseurs = collaborateurs.Count(c =>
                c.Direction == pc.Direction && c.Matching.HasValue && c.Matching > 0);

            if (nbSuccesseurs == 0)
                pcSansReleve++;
            else
                pcCouverts++;

            if (nbSuccesseurs < pc.MinSuccesseurs)
                gaps++;
        }

        double tauxCouverture = pcTotal > 0 ? Math.Round((double)pcCouverts / pcTotal * 100, 1) : 0;

        // Ready Now
        int readyNow = collaborateurs.Count(c => c.Readiness == "Ready Now");

        // Engagement moyen (seulement les collaborateurs avec évaluation soumise ou engagement > 0)
        var engagements = collaborateurs
            .Where(c => c.Engagement > 0)
            .Select(c => (double)c.Engagement)
            .ToList();
        double engagementMoyen = engagements.Count > 0 ? Math.Round(engagements.Average(), 1) : 0;

        // Mobilité (collaborateurs avec PosteCible = mobilité souhaitée)
        int souhaitsMobilite = collaborateurs.Count(c => !string.IsNullOrEmpty(c.PosteCible));
        double tauxMobilite = populationTotale > 0
            ? Math.Round((double)souhaitsMobilite / populationTotale * 100, 1) : 0;

        return new KpiResult
        {
            PopulationTotale = populationTotale,
            NombreTalentsValides = talentsValides,
            NombreHautsPotentiels = hautsPotentiels,
            NombreViviersActifs = viviersActifs,
            PostesCritiquesTotaux = pcTotal,
            PostesCritiquesCouverts = pcCouverts,
            PostesCritiquesSansReleve = pcSansReleve,
            GapsCritiques = gaps,
            TauxCouverture = tauxCouverture,
            NombreReadyNow = readyNow,
            TauxMobiliteInterne = tauxMobilite,
            NombreSouhaitsMobilite = souhaitsMobilite,
            EngagementMoyen = engagementMoyen
        };
    }
}
