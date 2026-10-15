using Microsoft.EntityFrameworkCore;
using Talent360.Application.Interfaces;
using Talent360.Domain.Entities;

namespace Talent360.Infrastructure.Repositories;

public class PlanDeveloppementRepository : IPlanDeveloppementRepository
{
    private readonly AppDbContext _context;

    public PlanDeveloppementRepository(AppDbContext context)
    {
        _context = context;
    }

    public PlanDeveloppement? ObtenirParCollaborateurId(int collaborateurId)
    {
        return _context.PlansDeveloppement
            .Include(p => p.Actions)
            .FirstOrDefault(p => p.CollaborateurId == collaborateurId);
    }

    public void Ajouter(PlanDeveloppement plan)
    {
        _context.PlansDeveloppement.Add(plan);
        _context.SaveChanges();
    }

    public void MettreAJourAvancement(int actionId, int pourcentage)
    {
        var action = _context.ActionsDeveloppement.Find(actionId);
        if (action != null)
        {
            action.PourcentageAvancement = pourcentage;
            _context.SaveChanges();
        }
    }
}
