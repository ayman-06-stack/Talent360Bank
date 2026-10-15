namespace Talent360.Domain.Entities;

/// <summary>
/// Plan de succession importé (onglet 07_SUCCESSION).
/// Relie un collaborateur successeur à un poste critique.
/// </summary>
public class ImportSuccession
{
    public int Id { get; set; }
    public string PosteCritiqueTitre { get; set; } = string.Empty;
    public string MatriculeSuccesseur { get; set; } = string.Empty;
    public string Readiness { get; set; } = string.Empty;      // Ready Now / < 1 an / 1-2 ans / > 2 ans
    public int Priorite { get; set; }                          // 1 = principal, 2 = secondaire…
    public DateTime DateMiseAJour { get; set; }
}
