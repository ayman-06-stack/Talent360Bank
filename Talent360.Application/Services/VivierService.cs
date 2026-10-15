using System;
using System.Collections.Generic;
using System.Linq;
using Talent360.Application.Interfaces;
using Talent360.Application.Models;
using Talent360.Domain.Entities;

namespace Talent360.Application.Services;

public class VivierService : IVivierService
{
    private readonly ICollaborateurRepository _collabRepo;

    private static readonly string[] Viviers = new[]
    {
        "Direction",
        "Management",
        "Digital",
        "Risques",
        "Expertise",
        "Commercial"
    };

    public VivierService(ICollaborateurRepository collabRepo)
    {
        _collabRepo = collabRepo;
    }

    public List<VivierSummary> ObtenirViviers()
    {
        var all = _collabRepo.ObtenirTous();
        var summaries = new List<VivierSummary>();

        foreach (var v in Viviers)
        {
            var members = all.Where(c => string.Equals(c.Vivier, v, StringComparison.OrdinalIgnoreCase)).ToList();
            var effectif = members.Count;
            var perfMoy = effectif > 0 ? members.Average(m => m.ScorePerformance) : 0.0;
            var potMoy = effectif > 0 ? members.Average(m => m.ScorePotentiel) : 0.0;
            var readyNow = members.Count(m => string.Equals(m.Readiness, "Ready Now", StringComparison.OrdinalIgnoreCase));
            var pct = effectif > 0 ? (readyNow * 100.0 / effectif) : 0.0;

            summaries.Add(new VivierSummary
            {
                Nom = v,
                Effectif = effectif,
                PerfMoyenne = Math.Round(perfMoy, 1),
                PotentielMoyen = Math.Round(potMoy, 1),
                ReadyNowCount = readyNow,
                ReadyNowPct = Math.Round(pct, 1)
            });
        }

        return summaries;
    }
}
