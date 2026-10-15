using Talent360.Application.Interfaces;
using Talent360.Application.Models;

namespace Talent360.Application.Services;

/// <summary>
/// Service de génération d'alertes RH selon les règles métier.
/// </summary>
public class AlerteService
{
    private readonly ICollaborateurRepository _collaborateurRepo;
    private readonly IPosteCritiqueRepository _posteCritiqueRepo;
    private readonly IPlanDeveloppementRepository _planRepo;

    public AlerteService(
        ICollaborateurRepository collaborateurRepo,
        IPosteCritiqueRepository posteCritiqueRepo,
        IPlanDeveloppementRepository planRepo)
    {
        _collaborateurRepo = collaborateurRepo;
        _posteCritiqueRepo = posteCritiqueRepo;
        _planRepo = planRepo;
    }

    public List<AlerteRH> GenererAlertes()
    {
        var alertes = new List<AlerteRH>();
        var collaborateurs = _collaborateurRepo.ObtenirTous();
        var postesCritiques = _posteCritiqueRepo.ObtenirTous();

        // ── Alertes postes critiques ──
        foreach (var pc in postesCritiques)
        {
            int nbSuccesseurs = collaborateurs.Count(c =>
                c.Direction == pc.Direction && c.Matching.HasValue && c.Matching > 0);

            if (nbSuccesseurs == 0)
            {
                alertes.Add(new AlerteRH
                {
                    Niveau = pc.Criticite == "Stratégique" ? NiveauAlerte.Critique : NiveauAlerte.Attention,
                    Message = $"Poste {pc.Criticite.ToLower()} sans successeur — {pc.Titre} ({pc.Direction})"
                });
            }
            else if (nbSuccesseurs == 1)
            {
                alertes.Add(new AlerteRH
                {
                    Niveau = NiveauAlerte.Attention,
                    Message = $"Un seul successeur identifié pour {pc.Titre} ({pc.Direction})"
                });
            }
        }

        // ── Alertes plans de développement en retard ──
        var talentsAvecVivier = collaborateurs.Where(c => !string.IsNullOrEmpty(c.Vivier)).ToList();
        foreach (var collab in talentsAvecVivier)
        {
            var plan = _planRepo.ObtenirParCollaborateurId(collab.Id);
            if (plan != null)
            {
                foreach (var action in plan.Actions)
                {
                    if (action.PourcentageAvancement < 30)
                    {
                        alertes.Add(new AlerteRH
                        {
                            Niveau = NiveauAlerte.Critique,
                            Message = $"Plan de développement en retard pour {collab.Matricule} — action « {action.Nom} » à {action.PourcentageAvancement}%"
                        });
                    }
                }
            }
        }

        // ── Nouveaux talents validés (4 plus récents) ──
        var nouveauxTalents = collaborateurs
            .Where(c => c.TalentValide)
            .OrderByDescending(c => c.Id)
            .Take(4)
            .ToList();

        foreach (var talent in nouveauxTalents)
        {
            alertes.Add(new AlerteRH
            {
                Niveau = NiveauAlerte.Info,
                Message = $"Nouveau talent identifié — {talent.Matricule} {talent.Prenom} {talent.Nom} ({talent.Poste})"
            });
        }

        // Tri par sévérité (Critique d'abord)
        alertes.Sort((a, b) => b.Niveau.CompareTo(a.Niveau));

        return alertes;
    }
}
