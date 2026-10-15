namespace Talent360.Domain.Entities;

/// <summary>
/// Mouvement de carrière importé (onglet 08_CARRIERE).
/// Utilisé pour la vigilance RH : aucunMouvement4Ans.
/// </summary>
public class ImportCarriere
{
    public int Id { get; set; }
    public string Matricule { get; set; } = string.Empty;
    public DateTime DateMouvement { get; set; }
    public string TypeMouvement { get; set; } = string.Empty;  // Promotion / Mobilité latérale / Changement de direction
    public string PosteAvant { get; set; } = string.Empty;
    public string PosteApres { get; set; } = string.Empty;
    public string DirectionAvant { get; set; } = string.Empty;
    public string DirectionApres { get; set; } = string.Empty;
}
