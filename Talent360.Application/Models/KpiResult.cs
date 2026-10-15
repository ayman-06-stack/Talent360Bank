namespace Talent360.Application.Models;

/// <summary>
/// Indicateurs clés de performance agrégés.
/// </summary>
public class KpiResult
{
    // Population
    public int PopulationTotale { get; set; }
    public int NombreTalentsValides { get; set; }
    public int NombreHautsPotentiels { get; set; }
    public int NombreViviersActifs { get; set; }

    // Postes critiques
    public int PostesCritiquesTotaux { get; set; }
    public int PostesCritiquesCouverts { get; set; }
    public int PostesCritiquesSansReleve { get; set; }
    public int GapsCritiques { get; set; }
    public double TauxCouverture { get; set; }

    // Readiness
    public int NombreReadyNow { get; set; }

    // Mobilité
    public double TauxMobiliteInterne { get; set; }
    public int NombreSouhaitsMobilite { get; set; }

    // Engagement
    public double EngagementMoyen { get; set; }
}
