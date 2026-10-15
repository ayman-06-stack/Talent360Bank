using Talent360.Application.Interfaces;
using Talent360.Domain.Entities;

namespace Talent360.Infrastructure.Repositories;

public class PosteCritiqueRepository : IPosteCritiqueRepository
{
    private readonly AppDbContext _context;

    public PosteCritiqueRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<PosteCritique> ObtenirTous()
    {
        return _context.PostesCritiques.ToList();
    }
}
