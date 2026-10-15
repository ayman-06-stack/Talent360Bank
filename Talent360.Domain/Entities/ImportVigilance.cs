namespace Talent360.Domain.Entities;

/// <summary>
/// Données de vigilance complémentaires (onglet 12_VIGILANCE).
/// Complète les flags calculés automatiquement depuis les autres imports.
/// </summary>
public class ImportVigilance
{
    public int Id { get; set; }
    public string Matricule { get; set; } = string.Empty;
    public DateTime DateEvaluation { get; set; }
    public bool BaissePerformance { get; set; }                // Flag : baisse ≥10 pts vs année N-1
    public double? ScorePerformanceN1 { get; set; }            // Score performance année précédente
    public string? Commentaire { get; set; }                   // Note du RH
}
