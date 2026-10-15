using Talent360.Domain.Entities;

namespace Talent360.Application.Interfaces;

/// <summary>
/// Repository pour les plans de développement.
/// </summary>
public interface IPlanDeveloppementRepository
{
    PlanDeveloppement? ObtenirParCollaborateurId(int collaborateurId);
    void Ajouter(PlanDeveloppement plan);
    void MettreAJourAvancement(int actionId, int pourcentage);
}
