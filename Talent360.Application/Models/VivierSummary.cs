namespace Talent360.Application.Models;

public class VivierSummary
{
    public string Nom { get; set; } = string.Empty;
    public int Effectif { get; set; }
    public double PerfMoyenne { get; set; }
    public double PotentielMoyen { get; set; }
    public int ReadyNowCount { get; set; }
    public double ReadyNowPct { get; set; }
}
