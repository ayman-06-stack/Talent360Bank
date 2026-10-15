namespace Talent360.Domain.Entities;

/// <summary>
/// Note d'une compétence issue de l'auto-évaluation.
/// Échelle 1-5 (sera normalisée vers 0-100 par ×20).
/// </summary>
public class CompetenceNote
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public double Note { get; set; }

    // FK vers l'auto-évaluation parente
    public int ImportAutoEvaluationId { get; set; }
    public ImportAutoEvaluation ImportAutoEvaluation { get; set; } = null!;
}

/// <summary>
/// Auto-évaluation importée depuis le CSV auto_evaluation.csv.
/// Contient le score de performance auto-évalué et les compétences notées.
/// </summary>
public class ImportAutoEvaluation
{
    public int Id { get; set; }
    public string Matricule { get; set; } = string.Empty;
    public DateTime DateEvaluation { get; set; }
    public double ScorePerformanceAutoEval { get; set; }
    public List<CompetenceNote> Competences { get; set; } = new();
}
