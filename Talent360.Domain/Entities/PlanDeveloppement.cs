namespace Talent360.Domain.Entities;

public class PlanDeveloppement
{
    public int Id { get; set; }
    public int CollaborateurId { get; set; }
    public string Objectif { get; set; } = string.Empty;

    // Navigation
    public Collaborateur Collaborateur { get; set; } = null!;
    public List<ActionDeveloppement> Actions { get; set; } = new();
}
