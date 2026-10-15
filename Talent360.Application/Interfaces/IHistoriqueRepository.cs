using Talent360.Domain.Entities;

namespace Talent360.Application.Interfaces;

/// <summary>
/// Repository pour l'accès aux historiques d'événements.
/// </summary>
public interface IHistoriqueRepository
{
    List<HistoriqueEvenement> ObtenirParMatricule(string matricule);
    void Ajouter(HistoriqueEvenement evenement);
}
