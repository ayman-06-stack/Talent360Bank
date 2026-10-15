using Talent360.Application.Enums;

namespace Talent360.Application.Interfaces;

/// <summary>
/// Moteur de calcul RH — toutes les règles métier centralisées.
/// Aucune logique de calcul ne doit exister en dehors de cette interface.
/// Tous les seuils et pondérations sont lus depuis Parametres (jamais codés en dur).
/// </summary>
public interface ICalculEngine
{
    // ── Performance ──

    /// <summary>
    /// Calcule le score de performance à partir des 5 sous-scores.
    /// Perf = Objectifs×0.40 + Competences×0.20 + Comportement×0.20 + Contribution×0.10 + Developpement×0.10
    /// </summary>
    double CalculerPerformance(double objectifs, double competences, double comportement,
        double contribution, double developpement);

    /// <summary>
    /// Détermine la catégorie de performance individuelle (Exceptionnelle/Élevée/Solide/À renforcer/Insuffisante).
    /// </summary>
    CategoriePerformance DeterminerCategoriePerformance(double scorePerformance);

    // ── Potentiel ──

    /// <summary>
    /// Calcule le score de potentiel à partir des 7 sous-scores.
    /// </summary>
    double CalculerPotentiel(double agiliteApprentissage, double leadership, double adaptabilite,
        double gestionComplexite, double mobilite, double visionStrategique, double autonomie);

    // ── 9-Box ──

    /// <summary>
    /// Détermine le tier (Faible/Moyen/Élevé) d'un score pour un axe de la matrice 9-Box.
    /// Utilise les seuils séparés Performance ou Potentiel.
    /// </summary>
    CategorieTier DeterminerTierPerf(double score);
    CategorieTier DeterminerTierPot(double score);

    /// <summary>
    /// Calcule la case 9-Box à partir des scores de performance et potentiel.
    /// </summary>
    CategorieNeufBox CalculerNeufBox(double performance, double potentiel);

    // ── Talent ──

    /// <summary>
    /// Détermine si un collaborateur est proposé comme Talent (Perf >=85 ET Pot >=85).
    /// </summary>
    bool EstTalentPropose(double performance, double potentiel);

    /// <summary>
    /// Détermine si un collaborateur est proposé comme Haut Potentiel (Pot >=85 ET Perf >=75).
    /// </summary>
    bool EstHautPotentielPropose(double performance, double potentiel);

    // ── Matching & Readiness ──

    /// <summary>
    /// Calcule le score de matching (adéquation à un poste cible).
    /// Matching = Competences×0.25 + Performance×0.20 + Potentiel×0.20 + Experience×0.15 + Leadership×0.10 + Mobilite×0.10
    /// ScoreExperience = min(PointsParAn × ancienneté, Plafond).
    /// </summary>
    double CalculerMatching(double competences, double performance, double potentiel,
        double experience, double leadership, double mobilite);

    /// <summary>
    /// Calcule le score d'expérience à partir de l'ancienneté en années.
    /// </summary>
    double CalculerScoreExperience(int anneesAnciennete);

    /// <summary>
    /// Détermine le niveau de readiness : "Ready Now" / "Ready < 1 an" / "Ready 1-2 ans" / "Ready > 2 ans".
    /// </summary>
    string DeterminerReadiness(double matching);

    // ── Vigilance RH ──

    /// <summary>
    /// Calcule l'indice de vigilance RH (0-100, système de points cumulatifs).
    /// ⚠ Ce score ne doit JAMAIS être présenté comme une prédiction de départ,
    /// uniquement comme un outil de priorisation d'actions d'accompagnement RH.
    /// </summary>
    int CalculerIndiceVigilance(double engagement, bool aucunMouvement4Ans,
        bool mobiliteNonTraitee, bool aucunDevRecent, bool baissePerformance,
        bool faibleReconnaissance, bool formationNonRealisee);

    /// <summary>
    /// Détermine le niveau de vigilance à partir de l'indice (Faible/Modérée/Élevée).
    /// </summary>
    NiveauVigilance DeterminerNiveauVigilance(int indice);

    // ── Gap de compétence ──

    /// <summary>
    /// Détermine le statut d'un gap de compétence (Maîtrise/À développer/Prioritaire).
    /// </summary>
    StatutGapCompetence DeterminerStatutGap(int niveauActuel, int niveauCible);
}
