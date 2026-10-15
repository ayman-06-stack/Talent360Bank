namespace Talent360.Application.Models;

public class ComiteMember
{
    public int Id { get; set; }
    public string NomComplet { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public string Poste { get; set; } = string.Empty;
    public double ScorePerformance { get; set; }
    public double ScorePotentiel { get; set; }
}
