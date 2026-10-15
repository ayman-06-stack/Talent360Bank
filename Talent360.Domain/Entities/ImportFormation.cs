namespace Talent360.Domain.Entities;

/// <summary>
/// Action de formation importée (onglet 05_FORMATION).
/// Utilisée pour la vigilance RH : formationNonRealisee / aucunDevRecent.
/// </summary>
public class ImportFormation
{
    public int Id { get; set; }
    public string Matricule { get; set; } = string.Empty;
    public DateTime DateFormation { get; set; }
    public string Intitule { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;          // Technique / Managériale / Comportementale
    public string Statut { get; set; } = string.Empty;        // Réalisée / Non réalisée / En cours
    public int DureeHeures { get; set; }
}
