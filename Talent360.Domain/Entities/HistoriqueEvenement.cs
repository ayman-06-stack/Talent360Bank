namespace Talent360.Domain.Entities;

public class HistoriqueEvenement
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Matricule { get; set; } = string.Empty;
    public string Acteur { get; set; } = string.Empty;
    public string Evenement { get; set; } = string.Empty;
}
