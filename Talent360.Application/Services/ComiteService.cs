using System;
using System.Collections.Generic;
using System.Linq;
using Talent360.Application.Interfaces;
using Talent360.Application.Models;

namespace Talent360.Application.Services;

public class ComiteService : IComiteService
{
    private readonly ICollaborateurRepository _collabRepo;

    public ComiteService(ICollaborateurRepository collabRepo)
    {
        _collabRepo = collabRepo;
    }

    public List<ComiteMember> ObtenirMembresProposes(int top = 20)
    {
        var all = _collabRepo.ObtenirTous();
        // Propose les collaborateurs identifiés comme talent (TalentPropose=true)
        // OU ceux avec ScorePerformance ET ScorePotentiel >= 80 (filet de sécurité)
        var candidates = all.Where(c => c.TalentPropose || (c.ScorePerformance >= 80 && c.ScorePotentiel >= 80))
            .OrderByDescending(c => (c.ScorePerformance + c.ScorePotentiel))
            .Take(top)
            .Select(c => new ComiteMember
            {
                Id = c.Id,
                NomComplet = $"{c.Prenom} {c.Nom}",
                Direction = c.Direction,
                Poste = c.Poste,
                ScorePerformance = Math.Round(c.ScorePerformance, 1),
                ScorePotentiel = Math.Round(c.ScorePotentiel, 1)
            })
            .ToList();

        return candidates;
    }
}
