using Talent360.Domain.Entities;

namespace Talent360.Desktop.Services;

public interface ISessionService
{
    Utilisateur? CurrentUser { get; set; }

    /// <summary>
    /// Efface la session courante (deconnexion).
    /// </summary>
    void Logout();

    /// <summary>
    /// Indique si un utilisateur est connecte.
    /// </summary>
    bool EstConnecte { get; }
}
