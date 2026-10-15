using Talent360.Application.Interfaces;
using Talent360.Domain.Entities;

namespace Talent360.Infrastructure.Repositories;

/// <summary>
/// Implémentation EF Core du repository des utilisateurs.
/// </summary>
public class UtilisateurRepository : IUtilisateurRepository
{
    private readonly AppDbContext _context;

    public UtilisateurRepository(AppDbContext context)
    {
        _context = context;
    }

    public Utilisateur? ObtenirParLogin(string login)
    {
        return _context.Utilisateurs.FirstOrDefault(u => u.Login == login);
    }

    public void IncrementerTentativesEchouees(string login)
    {
        var utilisateur = _context.Utilisateurs.FirstOrDefault(u => u.Login == login);
        if (utilisateur != null)
        {
            utilisateur.TentativesEchouees++;
            _context.SaveChanges();
        }
    }

    public void ReinitialiserTentatives(string login)
    {
        var utilisateur = _context.Utilisateurs.FirstOrDefault(u => u.Login == login);
        if (utilisateur != null)
        {
            utilisateur.TentativesEchouees = 0;
            _context.SaveChanges();
        }
    }

    public void VerrouillerCompte(string login)
    {
        var utilisateur = _context.Utilisateurs.FirstOrDefault(u => u.Login == login);
        if (utilisateur != null)
        {
            utilisateur.CompteVerrouille = true;
            _context.SaveChanges();
        }
    }
}
