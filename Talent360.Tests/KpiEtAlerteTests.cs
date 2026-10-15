using Talent360.Application.Enums;
using Talent360.Application.Interfaces;
using Talent360.Application.Models;
using Talent360.Application.Services;
using Talent360.Domain.Entities;

namespace Talent360.Tests;

// ═══════════════════════════════════════════════
//  STUBS pour les tests KPI et Alerte
// ═══════════════════════════════════════════════

internal class CollaborateurRepositoryStub : ICollaborateurRepository
{
    private readonly List<Collaborateur> _collaborateurs;

    public CollaborateurRepositoryStub(List<Collaborateur> collaborateurs)
    {
        _collaborateurs = collaborateurs;
    }

    public List<Collaborateur> ObtenirTous() => _collaborateurs;
    public Collaborateur? ObtenirParMatricule(string matricule) =>
        _collaborateurs.FirstOrDefault(c => c.Matricule == matricule);
    public List<Collaborateur> ObtenirParDirection(string direction) =>
        _collaborateurs.Where(c => c.Direction == direction).ToList();
    public void MettreAJourEvaluation(string matricule, double performance, double potentiel) { }
}

internal class PosteCritiqueRepositoryStub : IPosteCritiqueRepository
{
    private readonly List<PosteCritique> _postes;

    public PosteCritiqueRepositoryStub(List<PosteCritique> postes)
    {
        _postes = postes;
    }

    public List<PosteCritique> ObtenirTous() => _postes;
}

internal class PlanDeveloppementRepositoryStub : IPlanDeveloppementRepository
{
    private readonly List<PlanDeveloppement> _plans;

    public PlanDeveloppementRepositoryStub(List<PlanDeveloppement> plans)
    {
        _plans = plans;
    }

    public PlanDeveloppement? ObtenirParCollaborateurId(int collaborateurId) =>
        _plans.FirstOrDefault(p => p.CollaborateurId == collaborateurId);
    public void Ajouter(PlanDeveloppement plan) => _plans.Add(plan);
    public void MettreAJourAvancement(int actionId, int pourcentage) { }
}

// ═══════════════════════════════════════════════
//  TESTS KpiService
// ═══════════════════════════════════════════════

public class KpiServiceTests
{
    private static KpiService CreerService(
        List<Collaborateur> collaborateurs,
        List<PosteCritique> postesCritiques)
    {
        return new KpiService(
            new CollaborateurRepositoryStub(collaborateurs),
            new PosteCritiqueRepositoryStub(postesCritiques),
            new CalculEngine(new ParametreRepositoryStub()));
    }

    [Fact]
    public void Test_CalculerKpi_PopulationTotale_CorrecteAvec3Collaborateurs()
    {
        var collabs = new List<Collaborateur>
        {
            new() { Matricule = "BP001", Direction = "Finance", ScorePerformance = 90, ScorePotentiel = 85, Engagement = 80 },
            new() { Matricule = "BP002", Direction = "RH", ScorePerformance = 70, ScorePotentiel = 70, Engagement = 75 },
            new() { Matricule = "BP003", Direction = "IT & Digital", ScorePerformance = 50, ScorePotentiel = 50, Engagement = 60 }
        };
        var result = CreerService(collabs, new List<PosteCritique>()).CalculerKpi();

        Assert.Equal(3, result.PopulationTotale);
    }

    [Fact]
    public void Test_CalculerKpi_TalentsValides_CompteCorrectement()
    {
        var collabs = new List<Collaborateur>
        {
            new() { Matricule = "BP001", TalentValide = true, Engagement = 80 },
            new() { Matricule = "BP002", TalentValide = false, Engagement = 70 },
            new() { Matricule = "BP003", TalentValide = true, Engagement = 90 }
        };
        var result = CreerService(collabs, new List<PosteCritique>()).CalculerKpi();

        Assert.Equal(2, result.NombreTalentsValides);
    }

    [Fact]
    public void Test_CalculerKpi_PostesCritiques_SansReleve_Detecte()
    {
        var collabs = new List<Collaborateur>();
        var postes = new List<PosteCritique>
        {
            new() { Titre = "DG Adjoint", Direction = "Finance", Criticite = "Stratégique", MinSuccesseurs = 2 }
        };
        var result = CreerService(collabs, postes).CalculerKpi();

        Assert.Equal(1, result.PostesCritiquesSansReleve);
        Assert.Equal(0, result.TauxCouverture);
    }

    [Fact]
    public void Test_CalculerKpi_EngagementMoyen_CalculeCorrectemet()
    {
        var collabs = new List<Collaborateur>
        {
            new() { Matricule = "BP001", Engagement = 80 },
            new() { Matricule = "BP002", Engagement = 60 },
            new() { Matricule = "BP003", Engagement = 70 }
        };
        var result = CreerService(collabs, new List<PosteCritique>()).CalculerKpi();

        Assert.Equal(70, result.EngagementMoyen);
    }

    [Fact]
    public void Test_CalculerKpi_ReadyNow_CompteCorrectement()
    {
        var collabs = new List<Collaborateur>
        {
            new() { Matricule = "BP001", Readiness = "Ready Now", Engagement = 80 },
            new() { Matricule = "BP002", Readiness = "<2 ans", Engagement = 75 },
            new() { Matricule = "BP003", Readiness = "Ready Now", Engagement = 90 }
        };
        var result = CreerService(collabs, new List<PosteCritique>()).CalculerKpi();

        Assert.Equal(2, result.NombreReadyNow);
    }
}

// ═══════════════════════════════════════════════
//  TESTS AlerteService
// ═══════════════════════════════════════════════

public class AlerteServiceTests
{
    private static AlerteService CreerService(
        List<Collaborateur> collaborateurs,
        List<PosteCritique> postesCritiques,
        List<PlanDeveloppement>? plans = null)
    {
        return new AlerteService(
            new CollaborateurRepositoryStub(collaborateurs),
            new PosteCritiqueRepositoryStub(postesCritiques),
            new PlanDeveloppementRepositoryStub(plans ?? new List<PlanDeveloppement>()));
    }

    [Fact]
    public void Test_GenererAlertes_PosteStrategiqueSansSuccesseur_AlerteCritique()
    {
        var collabs = new List<Collaborateur>();
        var postes = new List<PosteCritique>
        {
            new() { Titre = "DG Adjoint", Direction = "Finance", Criticite = "Stratégique", MinSuccesseurs = 2 }
        };

        var alertes = CreerService(collabs, postes).GenererAlertes();

        Assert.Contains(alertes, a =>
            a.Niveau == NiveauAlerte.Critique &&
            a.Message.Contains("DG Adjoint") &&
            a.Message.Contains("sans successeur"));
    }

    [Fact]
    public void Test_GenererAlertes_PosteSensibleSansSuccesseur_AlerteAttention()
    {
        var collabs = new List<Collaborateur>();
        var postes = new List<PosteCritique>
        {
            new() { Titre = "Resp Conformité", Direction = "Risques", Criticite = "Sensible", MinSuccesseurs = 1 }
        };

        var alertes = CreerService(collabs, postes).GenererAlertes();

        Assert.Contains(alertes, a =>
            a.Niveau == NiveauAlerte.Attention &&
            a.Message.Contains("Resp Conformité") &&
            a.Message.Contains("sans successeur"));
    }

    [Fact]
    public void Test_GenererAlertes_PosteAvecUnSeulSuccesseur_AlerteAttention()
    {
        var collabs = new List<Collaborateur>
        {
            new() { Matricule = "BP001", Direction = "Finance", Matching = 85 }
        };
        var postes = new List<PosteCritique>
        {
            new() { Titre = "Directeur Financier", Direction = "Finance", Criticite = "Stratégique", MinSuccesseurs = 2 }
        };

        var alertes = CreerService(collabs, postes).GenererAlertes();

        Assert.Contains(alertes, a =>
            a.Niveau == NiveauAlerte.Attention &&
            a.Message.Contains("Un seul successeur"));
    }

    [Fact]
    public void Test_GenererAlertes_PlanEnRetard_AlerteCritique()
    {
        var collabs = new List<Collaborateur>
        {
            new() { Id = 1, Matricule = "BP001", Vivier = "Management" }
        };
        var plans = new List<PlanDeveloppement>
        {
            new()
            {
                CollaborateurId = 1,
                Objectif = "Préparer BP001",
                Actions = new List<ActionDeveloppement>
                {
                    new() { Nom = "Mentorat", PourcentageAvancement = 10 }
                }
            }
        };

        var alertes = CreerService(collabs, new List<PosteCritique>(), plans).GenererAlertes();

        Assert.Contains(alertes, a =>
            a.Niveau == NiveauAlerte.Critique &&
            a.Message.Contains("Plan de développement en retard") &&
            a.Message.Contains("Mentorat") &&
            a.Message.Contains("10%"));
    }

    [Fact]
    public void Test_GenererAlertes_NouveauTalent_AlerteInfo()
    {
        var collabs = new List<Collaborateur>
        {
            new() { Id = 1, Matricule = "BP001", Prenom = "Youssef", Nom = "Alaoui",
                    Poste = "Analyste", TalentValide = true }
        };

        var alertes = CreerService(collabs, new List<PosteCritique>()).GenererAlertes();

        Assert.Contains(alertes, a =>
            a.Niveau == NiveauAlerte.Info &&
            a.Message.Contains("Nouveau talent") &&
            a.Message.Contains("BP001"));
    }

    [Fact]
    public void Test_GenererAlertes_TrieParSeverite_CritiqueDabord()
    {
        var collabs = new List<Collaborateur>
        {
            new() { Id = 1, Matricule = "BP001", TalentValide = true, Vivier = "Direction",
                    Prenom = "A", Nom = "B", Poste = "P" }
        };
        var postes = new List<PosteCritique>
        {
            new() { Titre = "DG", Direction = "X", Criticite = "Stratégique", MinSuccesseurs = 1 }
        };
        var plans = new List<PlanDeveloppement>
        {
            new()
            {
                CollaborateurId = 1,
                Objectif = "Test",
                Actions = new List<ActionDeveloppement>
                {
                    new() { Nom = "Action", PourcentageAvancement = 5 }
                }
            }
        };

        var alertes = CreerService(collabs, postes, plans).GenererAlertes();

        // Vérifier que la première alerte est Critique
        Assert.True(alertes.Count > 0);
        Assert.Equal(NiveauAlerte.Critique, alertes[0].Niveau);

        // Vérifier qu'une Info vient après les Critiques/Attentions
        var lastInfo = alertes.FindLastIndex(a => a.Niveau == NiveauAlerte.Info);
        var lastCritique = alertes.FindLastIndex(a => a.Niveau == NiveauAlerte.Critique);
        if (lastInfo >= 0 && lastCritique >= 0)
            Assert.True(lastCritique < lastInfo, "Les alertes Critiques doivent apparaître avant les Info");
    }
}
