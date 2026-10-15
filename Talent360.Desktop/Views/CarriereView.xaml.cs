using System.Linq;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Talent360.Application.Interfaces;

namespace Talent360.Desktop.Views;

public partial class CarriereView : UserControl
{
    public CarriereView()
    {
        InitializeComponent();
        Charger();
    }

    private void Charger()
    {
        var app = (App)System.Windows.Application.Current;
        using var scope = app.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICollaborateurRepository>();

        var collabs = repo.ObtenirTous()
            .Where(c => !string.IsNullOrEmpty(c.PosteCible))
            .Select(c => new
            {
                c.Matricule,
                NomComplet = $"{c.Prenom} {c.Nom}",
                c.Direction,
                c.Poste,
                c.PosteCible,
                Readiness = c.Readiness ?? "—"
            })
            .ToList();

        GridMobilite.ItemsSource = collabs;
    }
}
