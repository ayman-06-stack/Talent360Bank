namespace Talent360.Domain.Entities;

/// <summary>
/// Questionnaire d'engagement importé depuis le CSV engagement.csv.
/// Les 6 dimensions sont sur une échelle 0-100.
/// </summary>
public class ImportEngagement
{
    public int Id { get; set; }
    public string Matricule { get; set; } = string.Empty;
    public DateTime DateReponse { get; set; }
    public double Management { get; set; }
    public double Reconnaissance { get; set; }
    public double Formation { get; set; }
    public double EvolutionPro { get; set; }
    public double ConditionsTravail { get; set; }
    public double Communication { get; set; }
}
