using Talent360.Application.Enums;
using Talent360.Application.Interfaces;

namespace Talent360.Application.Services;

/// <summary>
/// Implémentation du moteur de calcul RH — spécification banque v2.
/// Tous les seuils et pondérations sont lus depuis le repository de paramètres.
/// AUCUNE valeur n'est codée en dur.
/// </summary>
public class CalculEngine : ICalculEngine
{
    private readonly IParametreRepository _p;

    public CalculEngine(IParametreRepository parametres)
    {
        _p = parametres;
    }

    // ══════════════════════════════════════════════════
    //  PERFORMANCE
    // ══════════════════════════════════════════════════

    public double CalculerPerformance(double objectifs, double competences, double comportement,
        double contribution, double developpement)
    {
        double w1 = _p.ObtenirDouble("PoidsPerfObjectifs", 0.40);
        double w2 = _p.ObtenirDouble("PoidsPerfCompetences", 0.20);
        double w3 = _p.ObtenirDouble("PoidsPerfComportement", 0.20);
        double w4 = _p.ObtenirDouble("PoidsPerfContribution", 0.10);
        double w5 = _p.ObtenirDouble("PoidsPerfDeveloppement", 0.10);

        return Math.Round(
            objectifs * w1 + competences * w2 + comportement * w3 +
            contribution * w4 + developpement * w5, 1);
    }

    public CategoriePerformance DeterminerCategoriePerformance(double scorePerformance)
    {
        double seuilExcep = _p.ObtenirDouble("SeuilPerfExceptionnelle", 90);
        double seuilElev = _p.ObtenirDouble("SeuilPerfElevee", 80);
        double seuilSolide = _p.ObtenirDouble("SeuilPerfSolide", 70);
        double seuilRenf = _p.ObtenirDouble("SeuilPerfARenforcer", 60);

        if (scorePerformance >= seuilExcep) return CategoriePerformance.Exceptionnelle;
        if (scorePerformance >= seuilElev) return CategoriePerformance.Elevee;
        if (scorePerformance >= seuilSolide) return CategoriePerformance.Solide;
        if (scorePerformance >= seuilRenf) return CategoriePerformance.ARenforcer;
        return CategoriePerformance.Insuffisante;
    }

    // ══════════════════════════════════════════════════
    //  POTENTIEL
    // ══════════════════════════════════════════════════

    public double CalculerPotentiel(double agiliteApprentissage, double leadership, double adaptabilite,
        double gestionComplexite, double mobilite, double visionStrategique, double autonomie)
    {
        double w1 = _p.ObtenirDouble("PoidsPotAgiliteApprentissage", 0.20);
        double w2 = _p.ObtenirDouble("PoidsPotLeadership", 0.20);
        double w3 = _p.ObtenirDouble("PoidsPotAdaptabilite", 0.15);
        double w4 = _p.ObtenirDouble("PoidsPotGestionComplexite", 0.15);
        double w5 = _p.ObtenirDouble("PoidsPotMobilite", 0.10);
        double w6 = _p.ObtenirDouble("PoidsPotVisionStrategique", 0.10);
        double w7 = _p.ObtenirDouble("PoidsPotAutonomie", 0.10);

        return Math.Round(
            agiliteApprentissage * w1 + leadership * w2 + adaptabilite * w3 +
            gestionComplexite * w4 + mobilite * w5 + visionStrategique * w6 +
            autonomie * w7, 1);
    }

    // ══════════════════════════════════════════════════
    //  9-BOX
    // ══════════════════════════════════════════════════

    public CategorieTier DeterminerTierPerf(double score)
    {
        double seuilEleve = _p.ObtenirDouble("Seuil9BoxPerfEleve", 85);
        double seuilMoyen = _p.ObtenirDouble("Seuil9BoxPerfMoyen", 70);

        if (score >= seuilEleve) return CategorieTier.Eleve;
        if (score >= seuilMoyen) return CategorieTier.Moyen;
        return CategorieTier.Faible;
    }

    public CategorieTier DeterminerTierPot(double score)
    {
        double seuilEleve = _p.ObtenirDouble("Seuil9BoxPotEleve", 85);
        double seuilMoyen = _p.ObtenirDouble("Seuil9BoxPotMoyen", 70);

        if (score >= seuilEleve) return CategorieTier.Eleve;
        if (score >= seuilMoyen) return CategorieTier.Moyen;
        return CategorieTier.Faible;
    }

    public CategorieNeufBox CalculerNeufBox(double performance, double potentiel)
    {
        var tierPerf = DeterminerTierPerf(performance);
        var tierPot = DeterminerTierPot(potentiel);

        return (tierPerf, tierPot) switch
        {
            (CategorieTier.Eleve, CategorieTier.Eleve)   => CategorieNeufBox.TalentCle,
            (CategorieTier.Moyen, CategorieTier.Eleve)   => CategorieNeufBox.HautPotentiel,
            (CategorieTier.Faible, CategorieTier.Eleve)  => CategorieNeufBox.PotentielARevelier,
            (CategorieTier.Eleve, CategorieTier.Moyen)   => CategorieNeufBox.PilierSolide,
            (CategorieTier.Moyen, CategorieTier.Moyen)   => CategorieNeufBox.Confirme,
            (CategorieTier.Faible, CategorieTier.Moyen)  => CategorieNeufBox.ADevelopper,
            (CategorieTier.Eleve, CategorieTier.Faible)  => CategorieNeufBox.Expert,
            (CategorieTier.Moyen, CategorieTier.Faible)  => CategorieNeufBox.AAccompagner,
            (CategorieTier.Faible, CategorieTier.Faible) => CategorieNeufBox.ASurveiller,
            _ => CategorieNeufBox.ASurveiller
        };
    }

    // ══════════════════════════════════════════════════
    //  TALENT
    // ══════════════════════════════════════════════════

    public bool EstTalentPropose(double performance, double potentiel)
    {
        double seuilPerf = _p.ObtenirDouble("SeuilTalentProposePerf", 85);
        double seuilPot = _p.ObtenirDouble("SeuilTalentProposePot", 85);
        return performance >= seuilPerf && potentiel >= seuilPot;
    }

    public bool EstHautPotentielPropose(double performance, double potentiel)
    {
        double seuilPot = _p.ObtenirDouble("SeuilHautPotentielPot", 85);
        double seuilPerf = _p.ObtenirDouble("SeuilHautPotentielPerf", 75);
        return potentiel >= seuilPot && performance >= seuilPerf;
    }

    // ══════════════════════════════════════════════════
    //  MATCHING & READINESS
    // ══════════════════════════════════════════════════

    public double CalculerScoreExperience(int anneesAnciennete)
    {
        double pointsParAn = _p.ObtenirDouble("PointsExperienceParAn", 8);
        double plafond = _p.ObtenirDouble("PlafondExperience", 100);
        return Math.Min(plafond, anneesAnciennete * pointsParAn);
    }

    public double CalculerMatching(double competences, double performance, double potentiel,
        double experience, double leadership, double mobilite)
    {
        double w1 = _p.ObtenirDouble("PoidsMatchingCompetences", 0.25);
        double w2 = _p.ObtenirDouble("PoidsMatchingPerformance", 0.20);
        double w3 = _p.ObtenirDouble("PoidsMatchingPotentiel", 0.20);
        double w4 = _p.ObtenirDouble("PoidsMatchingExperience", 0.15);
        double w5 = _p.ObtenirDouble("PoidsMatchingLeadership", 0.10);
        double w6 = _p.ObtenirDouble("PoidsMatchingMobilite", 0.10);

        double brut = competences * w1 + performance * w2 + potentiel * w3 +
                      experience * w4 + leadership * w5 + mobilite * w6;

        return Math.Round(brut, 1);
    }

    public string DeterminerReadiness(double matching)
    {
        double seuilNow = _p.ObtenirDouble("SeuilReadinessNow", 90);
        double seuil1An = _p.ObtenirDouble("SeuilReadiness1An", 80);
        double seuil1a2 = _p.ObtenirDouble("SeuilReadiness1a2Ans", 65);

        if (matching >= seuilNow) return "Ready Now";
        if (matching >= seuil1An) return "Ready < 1 an";
        if (matching >= seuil1a2) return "Ready 1-2 ans";
        return "Ready > 2 ans";
    }

    // ══════════════════════════════════════════════════
    //  VIGILANCE RH (système de points cumulatifs)
    // ══════════════════════════════════════════════════

    public int CalculerIndiceVigilance(double engagement, bool aucunMouvement4Ans,
        bool mobiliteNonTraitee, bool aucunDevRecent, bool baissePerformance,
        bool faibleReconnaissance, bool formationNonRealisee)
    {
        int indice = 0;

        double seuilEng = _p.ObtenirDouble("SeuilVigilanceEngagement", 60);
        if (engagement < seuilEng)
            indice += _p.ObtenirInt("PointsVigilanceEngagement", 25);

        if (aucunMouvement4Ans)
            indice += _p.ObtenirInt("PointsVigilanceAucunMouvement", 20);

        if (mobiliteNonTraitee)
            indice += _p.ObtenirInt("PointsVigilanceMobiliteNonTraitee", 15);

        if (aucunDevRecent)
            indice += _p.ObtenirInt("PointsVigilanceAucunDev", 15);

        if (baissePerformance)
            indice += _p.ObtenirInt("PointsVigilanceBaissePerf", 10);

        if (faibleReconnaissance)
            indice += _p.ObtenirInt("PointsVigilanceFaibleReconnaissance", 10);

        if (formationNonRealisee)
            indice += _p.ObtenirInt("PointsVigilanceFormationNonRealisee", 5);

        return Math.Min(100, indice);
    }

    public NiveauVigilance DeterminerNiveauVigilance(int indice)
    {
        double seuilFaible = _p.ObtenirDouble("SeuilVigilanceFaible", 30);
        double seuilModere = _p.ObtenirDouble("SeuilVigilanceModere", 60);

        if (indice >= seuilModere) return NiveauVigilance.Elevee;
        if (indice >= seuilFaible) return NiveauVigilance.Moderee;
        return NiveauVigilance.Faible;
    }

    // ══════════════════════════════════════════════════
    //  GAP DE COMPÉTENCE
    // ══════════════════════════════════════════════════

    public StatutGapCompetence DeterminerStatutGap(int niveauActuel, int niveauCible)
    {
        int gap = niveauCible - niveauActuel;
        if (gap <= 0) return StatutGapCompetence.Maitrise;
        if (gap == 1) return StatutGapCompetence.ADevelopper;
        return StatutGapCompetence.Prioritaire;
    }
}
