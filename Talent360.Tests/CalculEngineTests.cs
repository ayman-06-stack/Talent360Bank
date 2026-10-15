using Talent360.Application.Enums;
using Talent360.Application.Interfaces;
using Talent360.Application.Services;

namespace Talent360.Tests;

/// <summary>
/// Stub de IParametreRepository avec les paramètres banque v2
/// (identiques à ceux du seeder). Pas de dépendance à la base de données.
/// </summary>
internal class ParametreRepositoryStub : IParametreRepository
{
    private readonly Dictionary<string, string> _parametres = new()
    {
        // Performance (5 axes)
        ["PoidsPerfObjectifs"] = "0.40",
        ["PoidsPerfCompetences"] = "0.20",
        ["PoidsPerfComportement"] = "0.20",
        ["PoidsPerfContribution"] = "0.10",
        ["PoidsPerfDeveloppement"] = "0.10",

        // Potentiel (7 axes)
        ["PoidsPotAgiliteApprentissage"] = "0.20",
        ["PoidsPotLeadership"] = "0.20",
        ["PoidsPotAdaptabilite"] = "0.15",
        ["PoidsPotGestionComplexite"] = "0.15",
        ["PoidsPotMobilite"] = "0.10",
        ["PoidsPotVisionStrategique"] = "0.10",
        ["PoidsPotAutonomie"] = "0.10",

        // Seuils 9-Box
        ["Seuil9BoxPerfEleve"] = "85",
        ["Seuil9BoxPerfMoyen"] = "70",
        ["Seuil9BoxPotEleve"] = "85",
        ["Seuil9BoxPotMoyen"] = "70",

        // Catégories Performance
        ["SeuilPerfExceptionnelle"] = "90",
        ["SeuilPerfElevee"] = "80",
        ["SeuilPerfSolide"] = "70",
        ["SeuilPerfARenforcer"] = "60",

        // Talent & HP
        ["SeuilTalentProposePerf"] = "85",
        ["SeuilTalentProposePot"] = "85",
        ["SeuilHautPotentielPot"] = "85",
        ["SeuilHautPotentielPerf"] = "75",

        // Matching
        ["PoidsMatchingCompetences"] = "0.25",
        ["PoidsMatchingPerformance"] = "0.20",
        ["PoidsMatchingPotentiel"] = "0.20",
        ["PoidsMatchingExperience"] = "0.15",
        ["PoidsMatchingLeadership"] = "0.10",
        ["PoidsMatchingMobilite"] = "0.10",
        ["PointsExperienceParAn"] = "8",
        ["PlafondExperience"] = "100",

        // Readiness
        ["SeuilReadinessNow"] = "90",
        ["SeuilReadiness1An"] = "80",
        ["SeuilReadiness1a2Ans"] = "65",

        // Vigilance
        ["PointsVigilanceEngagement"] = "25",
        ["SeuilVigilanceEngagement"] = "60",
        ["PointsVigilanceAucunMouvement"] = "20",
        ["SeuilVigilanceAnneesSansMouvement"] = "4",
        ["PointsVigilanceMobiliteNonTraitee"] = "15",
        ["PointsVigilanceAucunDev"] = "15",
        ["PointsVigilanceBaissePerf"] = "10",
        ["PointsVigilanceFaibleReconnaissance"] = "10",
        ["PointsVigilanceFormationNonRealisee"] = "5",
        ["SeuilVigilanceFaible"] = "30",
        ["SeuilVigilanceModere"] = "60"
    };

    public string Obtenir(string cle, string valeurParDefaut = "")
        => _parametres.TryGetValue(cle, out var val) ? val : valeurParDefaut;

    public double ObtenirDouble(string cle, double valeurParDefaut = 0)
        => _parametres.TryGetValue(cle, out var val)
            ? double.Parse(val, System.Globalization.CultureInfo.InvariantCulture)
            : valeurParDefaut;

    public int ObtenirInt(string cle, int valeurParDefaut = 0)
        => _parametres.TryGetValue(cle, out var val)
            ? int.Parse(val)
            : valeurParDefaut;
}

/// <summary>
/// Tests unitaires du moteur de calcul RH — spécification banque v2.
/// </summary>
public class CalculEngineTests
{
    private readonly CalculEngine _engine = new(new ParametreRepositoryStub());

    // ═══════════════════════════════════════════════
    //  TESTS CalculerPerformance
    // ═══════════════════════════════════════════════

    [Fact]
    public void Test_CalculerPerformance_ExempleSpec_Retourne61point6()
    {
        // Objectifs=64, Competences=50, Comportement=66, Contribution=63, Developpement=65
        // = 64×0.40 + 50×0.20 + 66×0.20 + 63×0.10 + 65×0.10
        // = 25.6 + 10 + 13.2 + 6.3 + 6.5 = 61.6
        var result = _engine.CalculerPerformance(64, 50, 66, 63, 65);
        Assert.Equal(61.6, result);
    }

    [Fact]
    public void Test_CalculerPerformance_TousA100_Retourne100()
    {
        var result = _engine.CalculerPerformance(100, 100, 100, 100, 100);
        Assert.Equal(100.0, result);
    }

    [Fact]
    public void Test_CalculerPerformance_TousA0_Retourne0()
    {
        var result = _engine.CalculerPerformance(0, 0, 0, 0, 0);
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Test_CalculerPerformance_PoidsCorrects()
    {
        // Objectifs=100, tout le reste à 0 → 100×0.40 = 40
        Assert.Equal(40.0, _engine.CalculerPerformance(100, 0, 0, 0, 0));
        // Competences=100, tout le reste à 0 → 100×0.20 = 20
        Assert.Equal(20.0, _engine.CalculerPerformance(0, 100, 0, 0, 0));
    }

    // ═══════════════════════════════════════════════
    //  TESTS DeterminerCategoriePerformance
    // ═══════════════════════════════════════════════

    [Theory]
    [InlineData(95, CategoriePerformance.Exceptionnelle)]
    [InlineData(90, CategoriePerformance.Exceptionnelle)]
    [InlineData(89, CategoriePerformance.Elevee)]
    [InlineData(80, CategoriePerformance.Elevee)]
    [InlineData(79, CategoriePerformance.Solide)]
    [InlineData(70, CategoriePerformance.Solide)]
    [InlineData(69, CategoriePerformance.ARenforcer)]
    [InlineData(61.6, CategoriePerformance.ARenforcer)]
    [InlineData(60, CategoriePerformance.ARenforcer)]
    [InlineData(59, CategoriePerformance.Insuffisante)]
    [InlineData(0, CategoriePerformance.Insuffisante)]
    public void Test_DeterminerCategoriePerformance(double score, CategoriePerformance attendu)
    {
        Assert.Equal(attendu, _engine.DeterminerCategoriePerformance(score));
    }

    // ═══════════════════════════════════════════════
    //  TESTS CalculerPotentiel
    // ═══════════════════════════════════════════════

    [Fact]
    public void Test_CalculerPotentiel_TousA80_Retourne80()
    {
        // 80×(0.20+0.20+0.15+0.15+0.10+0.10+0.10) = 80×1.0 = 80
        var result = _engine.CalculerPotentiel(80, 80, 80, 80, 80, 80, 80);
        Assert.Equal(80.0, result);
    }

    [Fact]
    public void Test_CalculerPotentiel_PoidsCorrects()
    {
        // Agilité=100, reste à 0 → 100×0.20 = 20
        Assert.Equal(20.0, _engine.CalculerPotentiel(100, 0, 0, 0, 0, 0, 0));
        // Leadership=100, reste à 0 → 100×0.20 = 20
        Assert.Equal(20.0, _engine.CalculerPotentiel(0, 100, 0, 0, 0, 0, 0));
        // Autonomie=100, reste à 0 → 100×0.10 = 10
        Assert.Equal(10.0, _engine.CalculerPotentiel(0, 0, 0, 0, 0, 0, 100));
    }

    // ═══════════════════════════════════════════════
    //  TESTS 9-Box (seuils 85/70)
    // ═══════════════════════════════════════════════

    [Theory]
    [InlineData(85, CategorieTier.Eleve)]
    [InlineData(95, CategorieTier.Eleve)]
    [InlineData(70, CategorieTier.Moyen)]
    [InlineData(84, CategorieTier.Moyen)]
    [InlineData(69, CategorieTier.Faible)]
    [InlineData(0, CategorieTier.Faible)]
    public void Test_DeterminerTierPerf_Seuils85_70(double score, CategorieTier attendu)
    {
        Assert.Equal(attendu, _engine.DeterminerTierPerf(score));
    }

    [Fact]
    public void Test_CalculerNeufBox_Perf90Pot90_TalentCle()
    {
        Assert.Equal(CategorieNeufBox.TalentCle, _engine.CalculerNeufBox(90, 90));
    }

    [Fact]
    public void Test_CalculerNeufBox_Perf75Pot88_HautPotentiel()
    {
        Assert.Equal(CategorieNeufBox.HautPotentiel, _engine.CalculerNeufBox(75, 88));
    }

    [Fact]
    public void Test_CalculerNeufBox_Perf50Pot90_PotentielARevelier()
    {
        Assert.Equal(CategorieNeufBox.PotentielARevelier, _engine.CalculerNeufBox(50, 90));
    }

    [Fact]
    public void Test_CalculerNeufBox_Perf88Pot75_PilierSolide()
    {
        Assert.Equal(CategorieNeufBox.PilierSolide, _engine.CalculerNeufBox(88, 75));
    }

    [Fact]
    public void Test_CalculerNeufBox_Perf75Pot75_Confirme()
    {
        Assert.Equal(CategorieNeufBox.Confirme, _engine.CalculerNeufBox(75, 75));
    }

    [Fact]
    public void Test_CalculerNeufBox_Perf50Pot75_ADevelopper()
    {
        Assert.Equal(CategorieNeufBox.ADevelopper, _engine.CalculerNeufBox(50, 75));
    }

    [Fact]
    public void Test_CalculerNeufBox_Perf90Pot50_Expert()
    {
        Assert.Equal(CategorieNeufBox.Expert, _engine.CalculerNeufBox(90, 50));
    }

    [Fact]
    public void Test_CalculerNeufBox_Perf75Pot50_AAccompagner()
    {
        Assert.Equal(CategorieNeufBox.AAccompagner, _engine.CalculerNeufBox(75, 50));
    }

    [Fact]
    public void Test_CalculerNeufBox_Perf50Pot50_ASurveiller()
    {
        Assert.Equal(CategorieNeufBox.ASurveiller, _engine.CalculerNeufBox(50, 50));
    }

    // ═══════════════════════════════════════════════
    //  TESTS Talent & Haut Potentiel
    // ═══════════════════════════════════════════════

    [Fact]
    public void Test_EstTalentPropose_Perf85Pot85_True()
    {
        Assert.True(_engine.EstTalentPropose(85, 85));
    }

    [Fact]
    public void Test_EstTalentPropose_Perf84Pot85_False()
    {
        Assert.False(_engine.EstTalentPropose(84, 85));
    }

    [Fact]
    public void Test_EstHautPotentielPropose_Pot85Perf75_True()
    {
        Assert.True(_engine.EstHautPotentielPropose(75, 85));
    }

    [Fact]
    public void Test_EstHautPotentielPropose_Pot85Perf74_False()
    {
        Assert.False(_engine.EstHautPotentielPropose(74, 85));
    }

    [Fact]
    public void Test_EstHautPotentielPropose_Pot84Perf90_False()
    {
        // Potentiel < 85 → false même avec perf élevée
        Assert.False(_engine.EstHautPotentielPropose(90, 84));
    }

    // ═══════════════════════════════════════════════
    //  TESTS Matching & Readiness
    // ═══════════════════════════════════════════════

    [Fact]
    public void Test_CalculerScoreExperience_10Ans_80Points()
    {
        // 10 × 8 = 80
        Assert.Equal(80, _engine.CalculerScoreExperience(10));
    }

    [Fact]
    public void Test_CalculerScoreExperience_15Ans_PlafonneA100()
    {
        // 15 × 8 = 120 → plafonné à 100
        Assert.Equal(100, _engine.CalculerScoreExperience(15));
    }

    [Fact]
    public void Test_CalculerMatching_TousA80_Retourne80()
    {
        // 80×0.25 + 80×0.20 + 80×0.20 + 80×0.15 + 80×0.10 + 80×0.10
        // = 20 + 16 + 16 + 12 + 8 + 8 = 80
        var result = _engine.CalculerMatching(80, 80, 80, 80, 80, 80);
        Assert.Equal(80.0, result);
    }

    [Fact]
    public void Test_CalculerMatching_CalculPondere()
    {
        // Comp=90, Perf=85, Pot=80, Exp=64, Lead=75, Mob=70
        // = 90×0.25 + 85×0.20 + 80×0.20 + 64×0.15 + 75×0.10 + 70×0.10
        // = 22.5 + 17 + 16 + 9.6 + 7.5 + 7.0 = 79.6
        var result = _engine.CalculerMatching(90, 85, 80, 64, 75, 70);
        Assert.Equal(79.6, result);
    }

    [Theory]
    [InlineData(95, "Ready Now")]
    [InlineData(90, "Ready Now")]
    [InlineData(85, "Ready < 1 an")]
    [InlineData(80, "Ready < 1 an")]
    [InlineData(70, "Ready 1-2 ans")]
    [InlineData(65, "Ready 1-2 ans")]
    [InlineData(64, "Ready > 2 ans")]
    [InlineData(50, "Ready > 2 ans")]
    public void Test_DeterminerReadiness(double matching, string attendu)
    {
        Assert.Equal(attendu, _engine.DeterminerReadiness(matching));
    }

    // ═══════════════════════════════════════════════
    //  TESTS Vigilance RH (points cumulatifs)
    // ═══════════════════════════════════════════════

    [Fact]
    public void Test_CalculerIndiceVigilance_AucunFacteur_Retourne0()
    {
        var indice = _engine.CalculerIndiceVigilance(70, false, false, false, false, false, false);
        Assert.Equal(0, indice);
    }

    [Fact]
    public void Test_CalculerIndiceVigilance_EngagementFaible_25Points()
    {
        var indice = _engine.CalculerIndiceVigilance(50, false, false, false, false, false, false);
        Assert.Equal(25, indice);
    }

    [Fact]
    public void Test_CalculerIndiceVigilance_AucunMouvement_20Points()
    {
        var indice = _engine.CalculerIndiceVigilance(70, true, false, false, false, false, false);
        Assert.Equal(20, indice);
    }

    [Fact]
    public void Test_CalculerIndiceVigilance_TousFacteurs_PlafonneA100()
    {
        // 25 + 20 + 15 + 15 + 10 + 10 + 5 = 100
        var indice = _engine.CalculerIndiceVigilance(50, true, true, true, true, true, true);
        Assert.Equal(100, indice);
    }

    [Fact]
    public void Test_CalculerIndiceVigilance_EngPlusMouvement_45Points()
    {
        // Engagement<60 (25) + AucunMouvement (20) = 45
        var indice = _engine.CalculerIndiceVigilance(55, true, false, false, false, false, false);
        Assert.Equal(45, indice);
    }

    [Theory]
    [InlineData(0, NiveauVigilance.Faible)]
    [InlineData(29, NiveauVigilance.Faible)]
    [InlineData(30, NiveauVigilance.Moderee)]
    [InlineData(59, NiveauVigilance.Moderee)]
    [InlineData(60, NiveauVigilance.Elevee)]
    [InlineData(100, NiveauVigilance.Elevee)]
    public void Test_DeterminerNiveauVigilance(int indice, NiveauVigilance attendu)
    {
        Assert.Equal(attendu, _engine.DeterminerNiveauVigilance(indice));
    }

    // ═══════════════════════════════════════════════
    //  TESTS Gap de compétence
    // ═══════════════════════════════════════════════

    [Theory]
    [InlineData(4, 3, StatutGapCompetence.Maitrise)]       // Gap = -1 → Maîtrise
    [InlineData(3, 3, StatutGapCompetence.Maitrise)]       // Gap = 0  → Maîtrise
    [InlineData(3, 4, StatutGapCompetence.ADevelopper)]     // Gap = 1  → À développer
    [InlineData(2, 4, StatutGapCompetence.Prioritaire)]     // Gap = 2  → Prioritaire
    [InlineData(1, 5, StatutGapCompetence.Prioritaire)]     // Gap = 4  → Prioritaire
    public void Test_DeterminerStatutGap(int actuel, int cible, StatutGapCompetence attendu)
    {
        Assert.Equal(attendu, _engine.DeterminerStatutGap(actuel, cible));
    }
}
