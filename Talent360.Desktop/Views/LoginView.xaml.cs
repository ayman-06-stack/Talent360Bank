using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Talent360.Desktop.ViewModels;

namespace Talent360.Desktop.Views;

public partial class LoginView : UserControl
{
    private readonly LoginViewModel _vm;

    /// <summary>
    /// Evenement declenche quand la connexion reussit.
    /// MainWindow s'abonne pour basculer vers le shell applicatif.
    /// </summary>
    public event System.Action? ConnexionReussie;

    public LoginView()
    {
        InitializeComponent();
        _vm = ((App)System.Windows.Application.Current).Services.GetRequiredService<LoginViewModel>();
        DataContext = _vm;
    }

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        var login = UsernameBox.Text?.Trim() ?? string.Empty;
        var password = PasswordBox.Password ?? string.Empty;

        LblErreur.Visibility = Visibility.Collapsed;

        bool ok = _vm.AttemptLogin(login, password);
        if (ok)
        {
            ConnexionReussie?.Invoke();
        }
    }

    /// <summary>
    /// Reinitialise le formulaire (apres deconnexion).
    /// </summary>
    public void Reinitialiser()
    {
        UsernameBox.Text = string.Empty;
        PasswordBox.Password = string.Empty;
        LblErreur.Visibility = Visibility.Collapsed;
    }
}
