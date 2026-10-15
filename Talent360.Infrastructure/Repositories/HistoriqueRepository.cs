using Talent360.Application.Interfaces;
using Talent360.Domain.Entities;

namespace Talent360.Infrastructure.Repositories;

public class HistoriqueRepository : IHistoriqueRepository
{
    private readonly AppDbContext _context;

    public HistoriqueRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<HistoriqueEvenement> ObtenirParMatricule(string matricule)
    {
        return _context.HistoriqueEvenements
            .Where(h => h.Matricule == matricule)
            .OrderByDescending(h => h.Date)
            .ToList();
    }

    public void Ajouter(HistoriqueEvenement evenement)
    {
        _context.HistoriqueEvenements.Add(evenement);
        _context.SaveChanges();
    }
}
