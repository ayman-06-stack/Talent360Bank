using System.Windows;
using Talent360.Application.Interfaces;
using Talent360.Desktop.Services;
using Talent360.Domain.Entities;

namespace Talent360.Desktop.ViewModels;

public class LoginViewModel
{
    private readonly IUtilisateurRepository _utilisateurRepository;
    private readonly ISessionService _sessionService;

    public string Login { get; set; } = string.Empty;

    public LoginViewModel(IUtilisateurRepository utilisateurRepository, ISessionService sessionService)
    {
        _utilisateurRepository = utilisateurRepository;
        _sessionService = sessionService;
    }

    public bool AttemptLogin(string login, string password)
    {
        var user = _utilisateurRepository.ObtenirParLogin(login);
        if (user == null)
        {
            MessageBox.Show("Identifiant inconnu.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (user.CompteVerrouille)
        {
            MessageBox.Show("Compte verrouillé. Contactez l'administrateur.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        bool ok = BCrypt.Net.BCrypt.Verify(password, user.MotDePasseHache);
        if (!ok)
        {
            _utilisateurRepository.IncrementerTentativesEchouees(login);
            if (user.TentativesEchouees + 1 >= 3)
            {
                _utilisateurRepository.VerrouillerCompte(login);
                MessageBox.Show("Trop de tentatives. Compte verrouillé.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show("Mot de passe incorrect.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return false;
        }

        // successful
        _utilisateurRepository.ReinitialiserTentatives(login);
        _sessionService.CurrentUser = user;
        return true;
    }
}
