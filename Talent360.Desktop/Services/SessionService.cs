using Talent360.Domain.Entities;

namespace Talent360.Desktop.Services;

public class SessionService : ISessionService
{
    public Utilisateur? CurrentUser { get; set; }

    public bool EstConnecte => CurrentUser != null;

    public void Logout()
    {
        CurrentUser = null;
    }
}
