using Talent360.Domain.Entities;

namespace Talent360.Application.Interfaces;

/// <summary>
/// Repository pour l'accès aux postes critiques.
/// </summary>
public interface IPosteCritiqueRepository
{
    List<PosteCritique> ObtenirTous();
}
