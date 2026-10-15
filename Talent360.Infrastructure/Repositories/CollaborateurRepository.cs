using Talent360.Application.Enums;
using Talent360.Application.Interfaces;
using Talent360.Domain.Entities;

namespace Talent360.Infrastructure.Repositories;

/// <summary>
/// Implémentation EF Core du repository des collaborateurs.
/// </summary>
public class CollaborateurRepository : ICollaborateurRepository
{
    private readonly AppDbContext _context;

    public CollaborateurRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<Collaborateur> ObtenirTous()
    {
        return CollaborateurSourceFilter.Apply(_context.Collaborateurs.AsQueryable()).ToList();
    }

    public Collaborateur? ObtenirParMatricule(string matricule)
    {
        return _context.Collaborateurs.FirstOrDefault(c => c.Matricule == matricule);
    }

    public List<Collaborateur> ObtenirParDirection(string direction)
    {
        return _context.Collaborateurs
            .Where(c => c.Direction == direction)
            .ToList();
    }

    public void MettreAJourEvaluation(string matricule, double performance, double potentiel)
    {
        var collab = _context.Collaborateurs.FirstOrDefault(c => c.Matricule == matricule);
        if (collab != null)
        {
            collab.ScorePerformance = performance;
            collab.ScorePotentiel = potentiel;
            _context.SaveChanges();
        }
    }
}
