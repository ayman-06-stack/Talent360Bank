using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Talent360.Application.Enums;
using Talent360.Application.Interfaces;

namespace Talent360.Desktop.ViewModels;

public partial class CollaborateurListViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<CollaborateurRow> _collaborateurs = new();
    [ObservableProperty] private int _nombreTotal;

    public CollaborateurListViewModel()
    {
        Refresh();
    }

    public void Refresh()
    {
        var app = (App)System.Windows.Application.Current;
        using var scope = app.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICollaborateurRepository>();

        var collabs = repo.ObtenirTous();
        NombreTotal = collabs.Count;

        Collaborateurs = new ObservableCollection<CollaborateurRow>(
            collabs.Select(c => new CollaborateurRow
            {
                Matricule = c.Matricule,
                Nom = c.Nom,
                Prenom = c.Prenom,
                Direction = c.Direction,
                Poste = c.Poste,
                Grade = c.Grade,
                ScorePerformance = c.ScorePerformance,
                ScorePotentiel = c.ScorePotentiel,
                StatutTalent = c.TalentValide ? "Validé" : c.TalentPropose ? "Proposé" : "—",
                Vivier = c.Vivier ?? "—"
            }));
    }
}

public class CollaborateurRow
{
    public string Matricule { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public string Poste { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public double ScorePerformance { get; set; }
    public double ScorePotentiel { get; set; }
    public string StatutTalent { get; set; } = string.Empty;
    public string Vivier { get; set; } = string.Empty;
}
