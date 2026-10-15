using Microsoft.EntityFrameworkCore;
using Talent360.Application.Interfaces;
using Talent360.Domain.Entities;

namespace Talent360.Infrastructure.Import;

/// <summary>
/// Service de recalcul des scores sur les Collaborateurs à partir des tables d'import.
/// Doit être appelé après chaque import de données.
///
/// Logique complète (12 sources) :
/// - ScorePerformance = moyenne pondérée (AutoEval 40% + Manager 60%), normalisée 0-100
/// - ScorePotentiel = CalculerPotentiel(7 axes) depuis ImportPotentiel, sinon estimation
/// - ScoreCompetences = moyenne des CompetenceNote ×20 (échelle 1-5 → 0-100)
/// - ScoreEngagement = moyenne des 6 dimensions ImportEngagement (déjà 0-100)
/// - TalentProposé / Vivier / Matching / Readiness recalculés via CalculEngine
/// - Matching utilise Leadership (ImportLeadership) et Mobilité (ImportMobilite)
/// - Vigilance calculée depuis Carrière, Formation, Mobilité, Reconnaissance, Vigilance
/// </summary>
public class ScoreRecalculService
{
    private readonly AppDbContext _context;
    private readonly ICalculEngine _calculEngine;
    private readonly IParametreRepository _parametres;

    private static readonly Dictionary<string, string> VivierMap = new()
    {
        ["Réseau commercial"] = "Commercial",
        ["Risques & conformité"] = "Risques",
        ["IT & Digital"] = "Digital",
        ["Finance"] = "Expertise",
        ["Audit"] = "Expertise",
        ["Juridique"] = "Expertise",
        ["RH"] = "Management",
        ["Opérations"] = "Management"
    };

    public ScoreRecalculService(AppDbContext context, ICalculEngine calculEngine)
    {
        _context = context;
        _calculEngine = calculEngine;
        // Le repository de paramètres est lu via le contexte directement ici
        _parametres = new Talent360.Infrastructure.Repositories.ParametreRepository(context);
    }

    /// <summary>
    /// Recalcule tous les scores de tous les collaborateurs à partir des imports.
    /// Retourne le nombre de collaborateurs mis à jour.
    /// </summary>
    public int RecalculerTout()
    {
        var collaborateurs = _context.Collaborateurs.ToList();
        if (!collaborateurs.Any()) return 0;

        // ── Charger toutes les données d'import en mémoire (groupées par matricule) ──
        var autoEvals = GrouperParMatricule(
            _context.ImportAutoEvaluations.Include(a => a.Competences).ToList(),
            a => a.Matricule);

        var evalManagers = GrouperParMatricule(
            _context.ImportEvaluationManagers.ToList(), e => e.Matricule);

        var engagements = GrouperParMatricule(
            _context.ImportEngagements.ToList(), e => e.Matricule);

        var potentiels = GrouperParMatricule(
            _context.ImportPotentiels.ToList(), p => p.Matricule);

        var leaderships = GrouperParMatricule(
            _context.ImportLeaderships.ToList(), l => l.Matricule);

        var mobilites = GrouperParMatricule(
            _context.ImportMobilites.ToList(), m => m.Matricule);

        var carrieres = GrouperParMatricule(
            _context.ImportCarrieres.ToList(), c => c.Matricule);

        var formations = GrouperParMatricule(
            _context.ImportFormations.ToList(), f => f.Matricule);

        var reconnaissances = GrouperParMatricule(
            _context.ImportReconnaissances.ToList(), r => r.Matricule);

        var vigilances = GrouperParMatricule(
            _context.ImportVigilances.ToList(), v => v.Matricule);

        int seuilAnneesSansMouvement = _parametres.ObtenirInt("SeuilVigilanceAnneesSansMouvement", 4);
        int updated = 0;

        foreach (var collab in collaborateurs)
        {
            bool modified = false;

            // ══════════════════════════════════════════
            //  SCORE PERFORMANCE
            // ══════════════════════════════════════════
            double? scoreAutoEval = null;
            double? scoreManager = null;

            if (autoEvals.TryGetValue(collab.Matricule, out var aes) && aes.Count > 0)
            {
                var derniere = aes.OrderByDescending(a => a.DateEvaluation).First();
                scoreAutoEval = derniere.ScorePerformanceAutoEval;
            }

            if (evalManagers.TryGetValue(collab.Matricule, out var ems) && ems.Count > 0)
            {
                var derniere = ems.OrderByDescending(e => e.DateEvaluation).First();
                scoreManager = derniere.ScorePerformanceManager;
            }

            if (scoreAutoEval.HasValue || scoreManager.HasValue)
            {
                if (scoreAutoEval.HasValue && scoreManager.HasValue)
                {
                    // Moyenne pondérée : 40% auto-éval + 60% manager
                    collab.ScorePerformance = Math.Round(scoreAutoEval.Value * 0.4 + scoreManager.Value * 0.6, 1);
                }
                else
                {
                    collab.ScorePerformance = Math.Round(scoreAutoEval ?? scoreManager!.Value, 1);
                }
                modified = true;
            }

            // ══════════════════════════════════════════
            //  SCORE COMPÉTENCES
            // ══════════════════════════════════════════
            if (autoEvals.TryGetValue(collab.Matricule, out var aesComp) && aesComp.Count > 0)
            {
                var derniere = aesComp.OrderByDescending(a => a.DateEvaluation).First();
                if (derniere.Competences.Count > 0)
                {
                    // Moyenne des notes × 20 (échelle 1-5 → 0-100)
                    collab.ScoreCompetences = Math.Round(derniere.Competences.Average(c => c.Note) * 20, 1);
                    modified = true;
                }
            }

            // ══════════════════════════════════════════
            //  SCORE ENGAGEMENT
            // ══════════════════════════════════════════
            if (engagements.TryGetValue(collab.Matricule, out var engs) && engs.Count > 0)
            {
                var derniere = engs.OrderByDescending(e => e.DateReponse).First();
                double moyEng = (derniere.Management + derniere.Reconnaissance + derniere.Formation
                    + derniere.EvolutionPro + derniere.ConditionsTravail + derniere.Communication) / 6.0;
                collab.ScoreEngagement = Math.Round(moyEng, 1);
                collab.Engagement = (int)Math.Round(moyEng);
                modified = true;
            }

            // ══════════════════════════════════════════
            //  SCORE POTENTIEL (via CalculerPotentiel si données disponibles)
            // ══════════════════════════════════════════
            double? potentiel7Axes = null;
            if (potentiels.TryGetValue(collab.Matricule, out var pots) && pots.Count > 0)
            {
                var derniere = pots.OrderByDescending(p => p.DateEvaluation).First();
                potentiel7Axes = _calculEngine.CalculerPotentiel(
                    derniere.AgiliteApprentissage, derniere.Leadership, derniere.Adaptabilite,
                    derniere.GestionComplexite, derniere.Mobilite, derniere.VisionStrategique,
                    derniere.Autonomie);
            }

            // Estimation basée sur Compétences + Engagement (formule RH standard)
            double potEstimation = Math.Round(collab.ScoreCompetences * 0.50
                                            + collab.ScoreEngagement * 0.50, 1);

            if (potentiel7Axes.HasValue)
            {
                // Prendre le MAX : ne pas pénaliser un collaborateur performant en compétences
                // et engagement à cause d'axes de niche (mobilité, vision stratégique) bas
                collab.ScorePotentiel = Math.Round(Math.Max(potentiel7Axes.Value, potEstimation), 1);
                modified = true;
            }
            else if (modified)
            {
                collab.ScorePotentiel = Math.Round(Math.Max(30, Math.Min(99, potEstimation)), 1);
            }

            // ══════════════════════════════════════════
            //  TALENT / VIVIER / MATCHING / READINESS
            // ══════════════════════════════════════════
            if (modified)
            {
                collab.TalentPropose = _calculEngine.EstTalentPropose(collab.ScorePerformance, collab.ScorePotentiel);
                collab.TalentValide = collab.TalentPropose; // auto-validé pour l'instant

                if (collab.TalentValide)
                {
                    collab.Vivier = VivierMap.GetValueOrDefault(collab.Direction, "Expertise");
                    int anneesAnc = (DateTime.Today - collab.DateEntree).Days / 365;
                    double scoreExp = _calculEngine.CalculerScoreExperience(anneesAnc);

                    // Leadership depuis ImportLeadership (ou 0 si pas de données)
                    double scoreLeadership = 0;
                    if (leaderships.TryGetValue(collab.Matricule, out var leads) && leads.Count > 0)
                    {
                        scoreLeadership = leads.OrderByDescending(l => l.DateEvaluation).First().ScoreLeadership;
                    }

                    // Mobilité depuis ImportMobilite (ou 0 si pas de données)
                    double scoreMobilite = 0;
                    if (mobilites.TryGetValue(collab.Matricule, out var mobs) && mobs.Count > 0)
                    {
                        scoreMobilite = mobs.OrderByDescending(m => m.DateDemande).First().ScoreMobilite;
                    }

                    collab.Matching = _calculEngine.CalculerMatching(
                        collab.ScoreCompetences, collab.ScorePerformance, collab.ScorePotentiel,
                        scoreExp, scoreLeadership, scoreMobilite);
                    collab.Readiness = _calculEngine.DeterminerReadiness(collab.Matching.Value);
                }
                else
                {
                    collab.Vivier = null;
                    collab.Matching = null;
                    collab.Readiness = null;
                }

                updated++;
            }
        }

        _context.SaveChanges();
        return updated;
    }

    /// <summary>
    /// Calcule les flags de vigilance pour un collaborateur à partir des imports.
    /// Utilisé par VigilanceView pour afficher l'indice détaillé.
    /// </summary>
    public (int Indice, bool EngagementFaible, bool AucunMouvement, bool MobiliteNonTraitee,
            bool AucunDevRecent, bool BaissePerformance, bool FaibleReconnaissance,
            bool FormationNonRealisee) CalculerFlagsVigilance(string matricule)
    {
        int seuilAnnees = _parametres.ObtenirInt("SeuilVigilanceAnneesSansMouvement", 4);
        double seuilEngagement = _parametres.ObtenirDouble("SeuilVigilanceEngagement", 60);
        var today = DateTime.Today;
        var dateLimite = today.AddYears(-seuilAnnees);
        var dateLimite2Ans = today.AddYears(-2);

        var collab = _context.Collaborateurs.FirstOrDefault(c =>
            c.Matricule == matricule);
        if (collab == null) return (0, false, false, false, false, false, false, false);

        // Flag 1 : Engagement < seuil
        bool engagementFaible = collab.ScoreEngagement < seuilEngagement;

        // Flag 2 : Aucun mouvement de carrière depuis N ans
        bool aucunMouvement = !_context.ImportCarrieres.Any(c =>
            c.Matricule == matricule && c.DateMouvement >= dateLimite);

        // Flag 3 : Demande de mobilité non traitée
        bool mobiliteNonTraitee = _context.ImportMobilites.Any(m =>
            m.Matricule == matricule &&
            m.Statut != null && m.Statut.ToLower() == "non traitée");

        // Flag 4 : Aucune action de développement (formation) récente (2 ans)
        bool aucunDevRecent = !_context.ImportFormations.Any(f =>
            f.Matricule == matricule && f.DateFormation >= dateLimite2Ans);

        // Flag 5 : Baisse de performance (depuis ImportVigilance ou comparaison)
        bool baissePerformance = _context.ImportVigilances.Any(v =>
            v.Matricule == matricule && v.BaissePerformance);

        // Flag 6 : Faible reconnaissance (aucune dans les 2 dernières années)
        bool faibleReconnaissance = !_context.ImportReconnaissances.Any(r =>
            r.Matricule == matricule && r.DateReconnaissance >= dateLimite2Ans);

        // Flag 7 : Formation non réalisée
        bool formationNonRealisee = _context.ImportFormations.Any(f =>
            f.Matricule == matricule &&
            f.Statut != null && f.Statut.ToLower() == "non réalisée");

        int indice = _calculEngine.CalculerIndiceVigilance(
            collab.ScoreEngagement, aucunMouvement, mobiliteNonTraitee,
            aucunDevRecent, baissePerformance, faibleReconnaissance, formationNonRealisee);

        return (indice, engagementFaible, aucunMouvement, mobiliteNonTraitee,
                aucunDevRecent, baissePerformance, faibleReconnaissance, formationNonRealisee);
    }

    // ── Helper ──
    private static Dictionary<string, List<T>> GrouperParMatricule<T>(
        List<T> items, Func<T, string> matriculeSelector)
    {
        return items
            .GroupBy(i => matriculeSelector(i), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);
    }
}
