using Talent360.Domain.Entities;

namespace Talent360.Application.Interfaces;

/// <summary>
/// Repository pour l'accès aux données des utilisateurs (authentification).
/// </summary>
public interface IUtilisateurRepository
{
    /// <summary>Retourne un utilisateur par son login, ou null.</summary>
    Utilisateur? ObtenirParLogin(string login);

    /// <summary>Incrémente le compteur de tentatives échouées.</summary>
    void IncrementerTentativesEchouees(string login);

    /// <summary>Réinitialise les tentatives échouées à zéro.</summary>
    void ReinitialiserTentatives(string login);

    /// <summary>Verrouille le compte de l'utilisateur.</summary>
    void VerrouillerCompte(string login);
}
