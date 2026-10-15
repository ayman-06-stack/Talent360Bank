using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Talent360.Application.Interfaces;

namespace Talent360.Desktop.ViewModels;

public partial class PosteCritiqueListViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<PosteCritiqueRow> _postes = new();
    [ObservableProperty] private bool _aDonnees;
    [ObservableProperty] private string _messageVide =
        "Aucun poste critique défini — les postes critiques sont créés lors de l'initialisation de la base de données.";

    public PosteCritiqueListViewModel()
    {
        var app = (App)System.Windows.Application.Current;
        using var scope = app.Services.CreateScope();
        var posteRepo = scope.ServiceProvider.GetRequiredService<IPosteCritiqueRepository>();
        var succession = scope.ServiceProvider.GetRequiredService<ISuccessionService>();

        var postes = posteRepo.ObtenirTous();
        ADonnees = postes.Any();

        var rows = postes.Select(p =>
        {
            var succ = succession.ProposerSuccesseurs(p.Id, 3);
            return new PosteCritiqueRow
            {
                Id = p.Id,
                Titre = p.Titre,
                Direction = p.Direction,
                Criticite = p.Criticite,
                SuccesseursTrouves = succ.Count,
                CouleurSuccesseurs = succ.Count == 0
                    ? new SolidColorBrush(Color.FromRgb(220, 38, 38))   // rouge
                    : succ.Count < 2
                        ? new SolidColorBrush(Color.FromRgb(217, 119, 6))  // orange
                        : new SolidColorBrush(Color.FromRgb(5, 150, 105))  // vert
            };
        }).ToList();

        Postes = new ObservableCollection<PosteCritiqueRow>(rows);
    }
}

public class PosteCritiqueRow
{
    public int Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public string Criticite { get; set; } = string.Empty;
    public int SuccesseursTrouves { get; set; }
    public Brush CouleurSuccesseurs { get; set; } = Brushes.Gray;
}
