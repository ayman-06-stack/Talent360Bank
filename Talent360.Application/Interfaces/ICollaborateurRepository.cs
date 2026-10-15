using Talent360.Domain.Entities;

namespace Talent360.Application.Interfaces;

/// <summary>
/// Repository pour l'accès aux données des collaborateurs.
/// </summary>
public interface ICollaborateurRepository
{
    /// <summary>Retourne tous les collaborateurs.</summary>
    List<Collaborateur> ObtenirTous();

    /// <summary>Retourne un collaborateur par son matricule, ou null.</summary>
    Collaborateur? ObtenirParMatricule(string matricule);

    /// <summary>Retourne les collaborateurs d'une direction donnée.</summary>
    List<Collaborateur> ObtenirParDirection(string direction);

    /// <summary>Met à jour les scores d'évaluation d'un collaborateur.</summary>
    void MettreAJourEvaluation(string matricule, double performance, double potentiel);
}
