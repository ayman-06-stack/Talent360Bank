using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using Talent360.Desktop.Services;
using Talent360.Desktop.Views;
using Talent360.Domain.Entities;

namespace Talent360.Desktop;

public partial class MainWindow : Window
{
    private Views.LoginView? _loginView;
    private ContentControl? _contentArea;
    private StackPanel? _sidebarPanel;

    public MainWindow()
    {
        InitializeComponent();
        AfficherLogin();
    }

    // ═══════════════════════════════════════════════════════
    //  NAVIGATION RACINE : Login <-> Shell
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Affiche l'ecran de connexion (sans sidebar).
    /// </summary>
    private void AfficherLogin()
    {
        _loginView = new Views.LoginView();
        _loginView.ConnexionReussie += OnConnexionReussie;
        RootContent.Content = _loginView;
    }

    /// <summary>
    /// Appele quand la connexion reussit : bascule vers le shell applicatif.
    /// </summary>
    private void OnConnexionReussie()
    {
        if (_loginView != null)
            _loginView.ConnexionReussie -= OnConnexionReussie;

        AfficherShell();
    }

    /// <summary>
    /// Construit et affiche le shell (sidebar + zone de contenu).
    /// </summary>
    private void AfficherShell()
    {
        // Structure : Grid avec sidebar a gauche + contenu a droite
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(240) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // ── SIDEBAR ──
        var sidebarBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(27, 42, 74)) // #1B2A4A
        };

        var sidebarDock = new DockPanel();

        // En-tete : logo
        var headerPanel = new StackPanel { Margin = new Thickness(0, 24, 0, 0) };
        headerPanel.Children.Add(new TextBlock
        {
            Text = "Talent360",
            FontSize = 22, FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 4)
        });
        headerPanel.Children.Add(new TextBlock
        {
            Text = "B A N K",
            FontSize = 14, FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(96, 165, 250)), // #60A5FA
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 12)
        });

        // Nom et role de l'utilisateur
        var session = ((App)System.Windows.Application.Current).Services
            .GetRequiredService<ISessionService>();
        var user = session.CurrentUser;
        if (user != null)
        {
            headerPanel.Children.Add(new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(37, 56, 90)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(12, 4, 12, 16),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = user.Login,
                            FontSize = 14, FontWeight = FontWeights.SemiBold,
                            Foreground = Brushes.White
                        },
                        new TextBlock
                        {
                            Text = user.Role.ToString(),
                            FontSize = 11,
                            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)) // #94A3B8
                        }
                    }
                }
            });
        }

        DockPanel.SetDock(headerPanel, Dock.Top);
        sidebarDock.Children.Add(headerPanel);

        // Bouton deconnexion (en bas)
        var btnDeconnexion = new Button
        {
            Content = "Se deconnecter",
            Foreground = new SolidColorBrush(Color.FromRgb(248, 113, 113)), // #F87171
            Background = Brushes.Transparent,
            FontSize = 13,
            Padding = new Thickness(16, 12, 16, 12),
            Cursor = System.Windows.Input.Cursors.Hand,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            BorderThickness = new Thickness(0),
            Margin = new Thickness(12, 8, 12, 16)
        };
        btnDeconnexion.Click += BtnDeconnexion_Click;
        DockPanel.SetDock(btnDeconnexion, Dock.Bottom);
        sidebarDock.Children.Add(btnDeconnexion);

        // Zone de navigation scrollable
        _sidebarPanel = new StackPanel();
        var scrollNav = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = _sidebarPanel
        };
        sidebarDock.Children.Add(scrollNav);

        sidebarBorder.Child = sidebarDock;
        Grid.SetColumn(sidebarBorder, 0);
        grid.Children.Add(sidebarBorder);

        // ── ZONE DE CONTENU ──
        _contentArea = new ContentControl();
        Grid.SetColumn(_contentArea, 1);
        grid.Children.Add(_contentArea);

        // Construire les boutons de navigation selon le role
        BuildSidebar();

        // Afficher le dashboard par defaut
        _contentArea.Content = new DashboardDrhView();

        RootContent.Content = grid;
    }

    // ═══════════════════════════════════════════════════════
    //  SIDEBAR — Construction selon le role
    // ═══════════════════════════════════════════════════════

    private void BuildSidebar()
    {
        if (_sidebarPanel == null) return;
        _sidebarPanel.Children.Clear();

        var session = ((App)System.Windows.Application.Current).Services
            .GetRequiredService<ISessionService>();
        var role = session.CurrentUser?.Role;

        void AddButton(string content, RoutedEventHandler handler)
        {
            var btn = new Button
            {
                Content = content,
                Style = (Style)FindResource("NavButtonStyle"),
                Margin = new Thickness(12, 2, 12, 2)
            };
            btn.Click += handler;
            _sidebarPanel.Children.Add(btn);
        }

        if (role == RoleUtilisateur.Collaborateur)
        {
            AddButton("Mon engagement", (s, e) => _contentArea!.Content = new PlaceholderView("Mon engagement"));
            AddButton("Campagne", (s, e) => _contentArea!.Content = new CampagneView());
        }
        else if (role == RoleUtilisateur.Manager)
        {
            AddButton("Campagne", (s, e) => _contentArea!.Content = new CampagneView());
            AddButton("Collaborateurs", (s, e) => _contentArea!.Content = new CollaborateurListView());
            AddButton("9-Box", (s, e) => _contentArea!.Content = new Box9View());
            AddButton("Talent Passport", (s, e) => _contentArea!.Content = new TalentPassportView());
            AddButton("Alertes", (s, e) => _contentArea!.Content = new PlaceholderView("Alertes"));
        }
        else if (role == RoleUtilisateur.DRH)
        {
            AddButton("Dashboard RH", (s, e) => _contentArea!.Content = new DashboardDrhView());
            AddButton("Synthese DG", (s, e) => _contentArea!.Content = new DashboardDgView());
            AddButton("Import de donnees", (s, e) => _contentArea!.Content = new ImportDonneesView());
            AddButton("Campagne", (s, e) => _contentArea!.Content = new CampagneView());
            AddButton("Collaborateurs", (s, e) => _contentArea!.Content = new CollaborateurListView());
            AddButton("9-Box", (s, e) => _contentArea!.Content = new Box9View());
            AddButton("Competences", (s, e) => _contentArea!.Content = new CompetenceView());
            AddButton("Carriere", (s, e) => _contentArea!.Content = new CarriereView());
            AddButton("Engagement", (s, e) => _contentArea!.Content = new EngagementView());
            AddButton("Vigilance RH", (s, e) => _contentArea!.Content = new VigilanceView());
            AddButton("Viviers", (s, e) => _contentArea!.Content = new VivierListView());
            AddButton("Postes critiques", (s, e) => _contentArea!.Content = new PosteCritiqueListView());
            AddButton("Comite", (s, e) => _contentArea!.Content = new ComiteView());
            AddButton("Talent Passport", (s, e) => _contentArea!.Content = new TalentPassportView());
            AddButton("Alertes", (s, e) => _contentArea!.Content = new PlaceholderView("Alertes"));
            AddButton("Historique", (s, e) => _contentArea!.Content = new PlaceholderView("Historique"));
        }
        else if (role == RoleUtilisateur.Comite)
        {
            AddButton("Synthese DG", (s, e) => _contentArea!.Content = new DashboardDgView());
            AddButton("Comite", (s, e) => _contentArea!.Content = new ComiteView());
            AddButton("Postes critiques", (s, e) => _contentArea!.Content = new PosteCritiqueListView());
            AddButton("Historique", (s, e) => _contentArea!.Content = new PlaceholderView("Historique"));
        }
        else
        {
            AddButton("Dashboard RH", (s, e) => _contentArea!.Content = new DashboardDrhView());
            AddButton("Collaborateurs", (s, e) => _contentArea!.Content = new CollaborateurListView());
        }
    }

    // ═══════════════════════════════════════════════════════
    //  DECONNEXION
    // ═══════════════════════════════════════════════════════

    private void BtnDeconnexion_Click(object sender, RoutedEventArgs e)
    {
        var result = System.Windows.MessageBox.Show(
            "Voulez-vous vous deconnecter ?",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        // Vider la session
        var session = ((App)System.Windows.Application.Current).Services
            .GetRequiredService<ISessionService>();
        session.Logout();

        // Revenir au login
        _sidebarPanel = null;
        _contentArea = null;
        AfficherLogin();
    }
}