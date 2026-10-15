using Talent360.Application.Interfaces;
using Talent360.Domain.Entities;

namespace Talent360.Application.Services;

public class SuccessionService : ISuccessionService
{
    private readonly IPosteCritiqueRepository _posteRepo;
    private readonly ICollaborateurRepository _collabRepo;
    private readonly ICalculEngine _calculEngine;

    public SuccessionService(IPosteCritiqueRepository posteRepo, ICollaborateurRepository collabRepo, ICalculEngine calculEngine)
    {
        _posteRepo = posteRepo;
        _collabRepo = collabRepo;
        _calculEngine = calculEngine;
    }

    public List<Collaborateur> ProposerSuccesseurs(int posteCritiqueId, int max = 3)
    {
        var postes = _posteRepo.ObtenirTous();
        var poste = postes.FirstOrDefault(p => p.Id == posteCritiqueId);
        if (poste == null) return new List<Collaborateur>();

        var candidats = _collabRepo.ObtenirParDirection(poste.Direction);
        if (candidats == null || candidats.Count == 0) return new List<Collaborateur>();

        // Use existing Matching if present, otherwise compute a fallback matching
        var scored = candidats.Select(c => new
        {
            Collaborateur = c,
            Score = c.Matching ?? _calculEngine.CalculerMatching(
                c.ScoreCompetences, c.ScorePerformance, c.ScorePotentiel,
                _calculEngine.CalculerScoreExperience((DateTime.Today - c.DateEntree).Days / 365),
                0, 0)  // Leadership et Mobilité non disponibles en fallback
        })
        .OrderByDescending(x => x.Score)
        .Select(x => x.Collaborateur)
        .Take(max)
        .ToList();

        return scored;
    }
}
