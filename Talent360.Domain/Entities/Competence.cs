namespace Talent360.Domain.Entities;

public class Competence
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Categorie { get; set; } = string.Empty;

    /// <summary>
    /// Note cible attendue (échelle 1-5). Modifiable par le DRH.
    /// </summary>
    public double NoteCible { get; set; } = 4.0;
}
