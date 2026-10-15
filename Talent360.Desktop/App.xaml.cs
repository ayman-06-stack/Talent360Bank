using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Talent360.Application.Interfaces;
using Talent360.Application.Services;
using Talent360.Infrastructure;
using Talent360.Infrastructure.Export;
using Talent360.Infrastructure.Repositories;
using Talent360.Infrastructure.Seed;

namespace Talent360.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // -- Gestionnaires globaux d'exceptions --
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        try
        {
            // Configuration du conteneur DI
            var services = new ServiceCollection();

            // Infrastructure
            services.AddDbContext<AppDbContext>();

            // Repositories (Scoped)
            services.AddScoped<IParametreRepository, ParametreRepository>();
            services.AddScoped<ICollaborateurRepository, CollaborateurRepository>();
            services.AddScoped<IUtilisateurRepository, UtilisateurRepository>();
            services.AddScoped<IPosteCritiqueRepository, PosteCritiqueRepository>();
            services.AddScoped<IHistoriqueRepository, HistoriqueRepository>();
            services.AddScoped<IPlanDeveloppementRepository, PlanDeveloppementRepository>();

            // Services metier (Scoped)
            services.AddScoped<ICalculEngine, CalculEngine>();
            services.AddScoped<KpiService>();
            services.AddScoped<AlerteService>();
            services.AddScoped<IExportService, ExcelExportService>();

            // Session : Singleton pour persister entre login/logout
            services.AddSingleton<Services.ISessionService, Services.SessionService>();
            services.AddTransient<ViewModels.LoginViewModel>();

            // Services metier supplementaires
            services.AddScoped<ISuccessionService, SuccessionService>();
            services.AddScoped<IVivierService, VivierService>();
            services.AddScoped<IComiteService, ComiteService>();

            // ViewModels
            services.AddTransient<ViewModels.VivierListViewModel>();
            services.AddTransient<ViewModels.ComiteViewModel>();
            services.AddTransient<ViewModels.VigilanceViewModel>();
            services.AddTransient<ViewModels.PosteCritiqueListViewModel>();
            services.AddTransient<ViewModels.Box9ViewModel>();

            Services = services.BuildServiceProvider();

            // Initialisation de la base de donnees + seed
            using (var scope = Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();

                try
                {
                    context.Database.ExecuteSqlRaw("ALTER TABLE Collaborateurs ADD COLUMN SourceDonnees INTEGER NOT NULL DEFAULT 0");
                }
                catch
                {
                    // La colonne existe deja sur les bases creees precedemment.
                }

                // Le seed de démonstration n'est plus exécuté automatiquement au démarrage.
                // Il doit être déclenché explicitement depuis l'interface si besoin.
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Erreur au demarrage :\n\n{ex.Message}\n\n{ex.InnerException?.Message}\n\n{ex.StackTrace}",
                "Talent360Bank -- Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
            return;
        }

        // Chargement des styles (chargement manuel, requis)
        this.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("Styles.xaml", UriKind.Relative)
        });

        // Fenetre unique : MainWindow affiche d'abord LoginView,
        // puis bascule vers le shell apres connexion reussie
        var mainWindow = new MainWindow();
        this.MainWindow = mainWindow;
        this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        mainWindow.Show();
    }


    // ═══════════════════════════════════════════════════════
    //  GESTIONNAIRES GLOBAUX D'EXCEPTIONS
    // ═══════════════════════════════════════════════════════

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        var msg = FormatException(e.Exception);
        EcrireLog(msg);
        System.Windows.MessageBox.Show(
            $"Une erreur inattendue s'est produite :\n\n{msg}",
            "Talent360Bank — Erreur",
            MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        var msg = ex != null ? FormatException(ex) : e.ExceptionObject?.ToString() ?? "Erreur inconnue";
        EcrireLog(msg);
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        e.SetObserved();
        var msg = FormatException(e.Exception);
        EcrireLog(msg);
    }

    private static string FormatException(Exception ex)
    {
        return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex.GetType().Name}: {ex.Message}\n" +
               $"StackTrace: {ex.StackTrace}\n" +
               (ex.InnerException != null
                   ? $"Inner: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}\n"
                   : "");
    }

    private static void EcrireLog(string message)
    {
        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var logDir = Path.Combine(appData, "Talent360Bank", "logs");
            Directory.CreateDirectory(logDir);
            var logPath = Path.Combine(logDir, "erreurs.log");
            File.AppendAllText(logPath, message + "\n---\n");
        }
        catch { /* ne pas propager une exception dans le handler d'exceptions */ }
    }
}