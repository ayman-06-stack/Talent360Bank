namespace Talent360.Application.Enums;

/// <summary>
/// Tier de la matrice 9-Box (axe performance ou potentiel).
/// Seuils : Élevé >=85, Moyen >=70, sinon Faible.
/// </summary>
public enum CategorieTier
{
    Faible,
    Moyen,
    Eleve
}

/// <summary>
/// Les 9 cases de la matrice Performance × Potentiel.
/// </summary>
public enum CategorieNeufBox
{
    TalentCle,          // (Eleve, Eleve)
    HautPotentiel,      // (Moyen, Eleve)
    PotentielARevelier, // (Faible, Eleve)
    PilierSolide,       // (Eleve, Moyen)
    Confirme,           // (Moyen, Moyen)
    ADevelopper,        // (Faible, Moyen)
    Expert,             // (Eleve, Faible)
    AAccompagner,       // (Moyen, Faible)
    ASurveiller         // (Faible, Faible)
}

/// <summary>
/// Catégorie de performance individuelle.
/// Exceptionnelle >=90, Élevée >=80, Solide >=70, À renforcer >=60, sinon Insuffisante.
/// </summary>
public enum CategoriePerformance
{
    Insuffisante,
    ARenforcer,
    Solide,
    Elevee,
    Exceptionnelle
}

/// <summary>
/// Niveau de vigilance RH (indice de priorisation d'accompagnement).
/// Faible <30, Modérée <60, Élevée >=60.
/// ⚠ Ce score ne doit JAMAIS être présenté comme une prédiction de départ.
/// </summary>
public enum NiveauVigilance
{
    Faible,
    Moderee,
    Elevee
}

/// <summary>
/// Statut d'écart de compétence (Gap = NiveauCible - NiveauActuel).
/// </summary>
public enum StatutGapCompetence
{
    Maitrise,      // Gap <= 0
    ADevelopper,   // Gap = 1
    Prioritaire    // Gap >= 2
}
