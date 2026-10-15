using Talent360.Domain.Entities;

namespace Talent360.Application.Enums;

public enum CollaborateurFiltreSource
{
    Tout = 0,
    DonneesReellesUniquement = 1,
    DemonstrationUniquement = 2
}

public static class CollaborateurSourceFilter
{
    public static CollaborateurFiltreSource Current { get; set; } = CollaborateurFiltreSource.DonneesReellesUniquement;

    public static IQueryable<Collaborateur> Apply(IQueryable<Collaborateur> query)
    {
        return Current switch
        {
            CollaborateurFiltreSource.Tout => query,
            CollaborateurFiltreSource.DemonstrationUniquement => query.Where(c => c.SourceDonnees == SourceDonnees.Demonstration),
            _ => query.Where(c => c.SourceDonnees == SourceDonnees.ImportReel)
        };
    }
}
