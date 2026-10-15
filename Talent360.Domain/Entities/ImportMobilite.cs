namespace Talent360.Domain.Entities;

/// <summary>
/// Demande de mobilité importée (onglet 06_MOBILITE).
/// Utilisée pour la vigilance RH (mobiliteNonTraitee) et le score Matching (axe mobilité).
/// </summary>
public class ImportMobilite
{
    public int Id { get; set; }
    public string Matricule { get; set; } = string.Empty;
    public DateTime DateDemande { get; set; }
    public string TypeMobilite { get; set; } = string.Empty;   // Géographique / Fonctionnelle / Internationale
    public string Statut { get; set; } = string.Empty;         // Traitée / Non traitée / En attente
    public double ScoreMobilite { get; set; }                  // Score de flexibilité mobilité 0-100
}
