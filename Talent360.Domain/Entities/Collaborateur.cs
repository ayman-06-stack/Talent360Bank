namespace Talent360.Domain.Entities;

public class Collaborateur
{
    public int Id { get; set; }
    public string Matricule { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public string Poste { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public int? ManagerId { get; set; }
    public SourceDonnees SourceDonnees { get; set; } = SourceDonnees.ImportReel;
    public DateTime DateEntree { get; set; }
    public double ScorePerformance { get; set; }
    public double ScorePotentiel { get; set; }
    public double ScoreCompetences { get; set; }
    public double ScoreEngagement { get; set; }

    // Talent & Vivier
    public bool TalentPropose { get; set; }
    public bool TalentValide { get; set; }
    public string? Vivier { get; set; }
    public string? PosteCible { get; set; }
    public double? Matching { get; set; }
    public string? Readiness { get; set; }

    // Engagement & Évaluation
    public int Engagement { get; set; }
    public bool EstSoumise { get; set; }

    // Navigation
    public PlanDeveloppement? PlanDeveloppement { get; set; }
}

