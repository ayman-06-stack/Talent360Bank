namespace Talent360.Domain.Entities;

public class ActionDeveloppement
{
    public int Id { get; set; }
    public int PlanDeveloppementId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public int PourcentageAvancement { get; set; }

    // Navigation
    public PlanDeveloppement PlanDeveloppement { get; set; } = null!;
}
