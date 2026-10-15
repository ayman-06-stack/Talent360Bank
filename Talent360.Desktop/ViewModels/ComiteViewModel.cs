using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Talent360.Application.Interfaces;
using Talent360.Application.Models;

namespace Talent360.Desktop.ViewModels;

public partial class ComiteViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ComiteMember> _membres = new();
    [ObservableProperty] private bool _aDonnees;
    [ObservableProperty] private string _messageVide =
        "Aucun collaborateur ne remplit les critères du comité (Performance ≥ 80 ET Potentiel ≥ 80). Importez les données d'évaluation pour alimenter cette vue.";

    public ComiteViewModel()
    {
        var app = (App)System.Windows.Application.Current;
        using var scope = app.Services.CreateScope();
        var comiteService = scope.ServiceProvider.GetRequiredService<IComiteService>();

        var list = comiteService.ObtenirMembresProposes();
        ADonnees = list.Any();
        Membres = new ObservableCollection<ComiteMember>(list);
    }
}
