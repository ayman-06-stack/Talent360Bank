namespace Talent360.Domain.Entities;

/// <summary>
/// Score de leadership 360° importé (onglet 10_LEADERSHIP).
/// Utilisé pour le Matching (axe leadership).
/// </summary>
public class ImportLeadership
{
    public int Id { get; set; }
    public string Matricule { get; set; } = string.Empty;
    public DateTime DateEvaluation { get; set; }
    public double ScoreLeadership { get; set; }                // Score global 0-100
    public double ScoreEquipe { get; set; }                    // Évaluation par l'équipe
    public double ScorePairs { get; set; }                     // Évaluation par les pairs
    public double ScoreHierarchie { get; set; }                // Évaluation par la hiérarchie
}
