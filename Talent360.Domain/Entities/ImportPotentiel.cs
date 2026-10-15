namespace Talent360.Domain.Entities;

/// <summary>
/// Évaluation de potentiel importée (onglet 04_POTENTIEL).
/// Les 7 dimensions sont sur une échelle 0-100.
/// </summary>
public class ImportPotentiel
{
    public int Id { get; set; }
    public string Matricule { get; set; } = string.Empty;
    public DateTime DateEvaluation { get; set; }
    public double AgiliteApprentissage { get; set; }
    public double Leadership { get; set; }
    public double Adaptabilite { get; set; }
    public double GestionComplexite { get; set; }
    public double Mobilite { get; set; }
    public double VisionStrategique { get; set; }
    public double Autonomie { get; set; }
}
