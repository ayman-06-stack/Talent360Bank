using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Talent360.Application.Interfaces;
using Talent360.Application.Models;

namespace Talent360.Desktop.ViewModels;

public partial class VivierListViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<VivierSummary> _viviers = new();
    [ObservableProperty] private bool _aDonnees;
    [ObservableProperty] private string _messageVide =
        "Aucun vivier actif — les viviers se remplissent automatiquement lorsque des collaborateurs atteignent les seuils de talent (Performance ≥ 80 et Potentiel ≥ 80).";

    public VivierListViewModel()
    {
        var app = (App)System.Windows.Application.Current;
        using var scope = app.Services.CreateScope();
        var vivierService = scope.ServiceProvider.GetRequiredService<IVivierService>();

        var list = vivierService.ObtenirViviers();
        var nonVides = list.Where(v => v.Effectif > 0).ToList();

        ADonnees = nonVides.Any();
        Viviers = new ObservableCollection<VivierSummary>(nonVides);
    }
}
