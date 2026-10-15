namespace Talent360.Domain.Entities;

/// <summary>
/// Événement de reconnaissance importé (onglet 09_RECONNAISSANCE).
/// Utilisé pour la vigilance RH : faibleReconnaissance.
/// </summary>
public class ImportReconnaissance
{
    public int Id { get; set; }
    public string Matricule { get; set; } = string.Empty;
    public DateTime DateReconnaissance { get; set; }
    public string Type { get; set; } = string.Empty;           // Prime / Bonus / Mention / Promotion / Autre
    public string Description { get; set; } = string.Empty;
}
