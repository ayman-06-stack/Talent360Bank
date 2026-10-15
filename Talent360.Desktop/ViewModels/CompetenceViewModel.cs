using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Talent360.Application.Enums;
using Talent360.Application.Interfaces;
using Talent360.Infrastructure;

namespace Talent360.Desktop.ViewModels;

/// <summary>
/// Ligne d'affichage pour une compétence avec note moyenne, cible et écart.
/// </summary>
public class CompetenceLigneVm
{
    public string Nom { get; set; } = string.Empty;
    public double NoteActuelle { get; set; }
    public double NoteCible { get; set; }
    public double Ecart { get; set; }
    public int NombreEvaluations { get; set; }
    public Brush CouleurEcart { get; set; } = Brushes.Gray;
    public string EcartTexte { get; set; } = string.Empty;
    public string StatutGap { get; set; } = string.Empty;
}

public partial class CompetenceViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<CompetenceLigneVm> _competences = new();
    [ObservableProperty] private bool _aDonnees;
    [ObservableProperty] private string _messageVide = "Aucune donnée de compétence importée — importez des auto-évaluations via l'écran Import de données.";

    // Formulaire de modification de la cible
    [ObservableProperty] private CompetenceLigneVm? _competenceSelectionnee;
    [ObservableProperty] private string _nouvelleCible = "4.0";

    public CompetenceViewModel()
    {
        Charger();
    }

    private void Charger()
    {
        var app = (App)System.Windows.Application.Current;
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var calculEngine = scope.ServiceProvider.GetRequiredService<ICalculEngine>();

        // Récupérer toutes les notes de compétences depuis les auto-évaluations importées
        var toutesNotes = context.CompetenceNotes.ToList();

        if (!toutesNotes.Any())
        {
            ADonnees = false;
            return;
        }

        ADonnees = true;

        // Récupérer le référentiel de compétences pour les notes cibles
        var referentiel = context.Competences.ToDictionary(c => c.Nom, c => c.NoteCible, StringComparer.OrdinalIgnoreCase);

        // Grouper par nom de compétence et calculer la moyenne
        var groupes = toutesNotes
            .GroupBy(cn => cn.Nom, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                double noteActuelle = Math.Round(g.Average(cn => cn.Note), 2);
                double noteCible = referentiel.TryGetValue(g.Key, out var cible) ? cible : 4.0;
                double ecart = Math.Round(noteActuelle - noteCible, 2);

                // Utiliser le CalculEngine pour le statut du gap
                var statut = calculEngine.DeterminerStatutGap((int)Math.Round(noteActuelle), (int)Math.Round(noteCible));

                return new CompetenceLigneVm
                {
                    Nom = g.Key,
                    NoteActuelle = noteActuelle,
                    NoteCible = noteCible,
                    Ecart = ecart,
                    NombreEvaluations = g.Count(),
                    StatutGap = statut switch
                    {
                        StatutGapCompetence.Maitrise => "Maitrise",
                        StatutGapCompetence.ADevelopper => "A developper",
                        StatutGapCompetence.Prioritaire => "Prioritaire",
                        _ => "—"
                    },
                    CouleurEcart = statut switch
                    {
                        StatutGapCompetence.Maitrise => new SolidColorBrush(Color.FromRgb(5, 150, 105)),
                        StatutGapCompetence.ADevelopper => new SolidColorBrush(Color.FromRgb(217, 119, 6)),
                        StatutGapCompetence.Prioritaire => new SolidColorBrush(Color.FromRgb(220, 38, 38)),
                        _ => new SolidColorBrush(Color.FromRgb(107, 114, 128))
                    },
                    EcartTexte = ecart > 0 ? $"+{ecart:F2}" : $"{ecart:F2}"
                };
            })
            .OrderBy(c => c.Ecart)
            .ToList();

        Competences = new ObservableCollection<CompetenceLigneVm>(groupes);
    }

    [RelayCommand]
    private void ModifierCible()
    {
        if (CompetenceSelectionnee == null) return;
        if (!double.TryParse(NouvelleCible, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var valeur))
        {
            MessageBox.Show("Veuillez saisir un nombre valide (ex: 3.5)", "Erreur",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (valeur < 1 || valeur > 5)
        {
            MessageBox.Show("La note cible doit être entre 1 et 5.", "Erreur",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var app = (App)System.Windows.Application.Current;
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Chercher ou créer la compétence dans le référentiel
        var comp = context.Competences.FirstOrDefault(c =>
            c.Nom.ToLower() == CompetenceSelectionnee.Nom.ToLower());

        if (comp != null)
        {
            comp.NoteCible = valeur;
        }
        else
        {
            context.Competences.Add(new Domain.Entities.Competence
            {
                Nom = CompetenceSelectionnee.Nom,
                Categorie = "Import",
                NoteCible = valeur
            });
        }

        context.SaveChanges();

        // Rafraîchir l'affichage
        Charger();
    }
}
