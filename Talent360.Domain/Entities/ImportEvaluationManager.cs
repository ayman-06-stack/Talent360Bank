namespace Talent360.Domain.Entities;

/// <summary>
/// Évaluation manager importée depuis le CSV evaluation_manager.csv.
/// </summary>
public class ImportEvaluationManager
{
    public int Id { get; set; }
    public string Matricule { get; set; } = string.Empty;
    public DateTime DateEvaluation { get; set; }
    public double ScorePerformanceManager { get; set; }
}
