namespace Talent360.Application.Models;

/// <summary>
/// Alerte générée par le système de vigilance RH.
/// </summary>
public class AlerteRH
{
    public NiveauAlerte Niveau { get; set; }
    public string Message { get; set; } = string.Empty;
}

public enum NiveauAlerte
{
    Info,
    Attention,
    Critique
}
