using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Talent360.Application.Interfaces;
using Talent360.Application.Enums;
using Talent360.Domain.Entities;

namespace Talent360.Desktop.ViewModels;

public partial class Box9ViewModel : ObservableObject
{
    public record BoxCell(CategorieNeufBox Category, string Label, int Count);

    public ObservableCollection<BoxCell> Cells { get; } = new();

    private readonly ICollaborateurRepository _collabRepo;
    private readonly ICalculEngine _calcul;
    private readonly Dictionary<CategorieNeufBox, List<Collaborateur>> _members = new();

    public Box9ViewModel(ICollaborateurRepository collabRepo, ICalculEngine calcul)
    {
        _collabRepo = collabRepo;
        _calcul = calcul;

        Load();
    }

    private void Load()
    {
        _members.Clear();
        foreach (CategorieNeufBox c in Enum.GetValues(typeof(CategorieNeufBox)))
            _members[c] = new List<Collaborateur>();

        var all = _collabRepo.ObtenirTous();
        foreach (var coll in all)
        {
            var cat = _calcul.CalculerNeufBox(coll.ScorePerformance, coll.ScorePotentiel);
            _members[cat].Add(coll);
        }

        Cells.Clear();
        Cells.Add(new BoxCell(CategorieNeufBox.TalentCle, "Talent clé", _members[CategorieNeufBox.TalentCle].Count));
        Cells.Add(new BoxCell(CategorieNeufBox.HautPotentiel, "Haut potentiel", _members[CategorieNeufBox.HautPotentiel].Count));
        Cells.Add(new BoxCell(CategorieNeufBox.PotentielARevelier, "Potentiel à révéler", _members[CategorieNeufBox.PotentielARevelier].Count));

        Cells.Add(new BoxCell(CategorieNeufBox.PilierSolide, "Pilier solide", _members[CategorieNeufBox.PilierSolide].Count));
        Cells.Add(new BoxCell(CategorieNeufBox.Confirme, "Confirmé", _members[CategorieNeufBox.Confirme].Count));
        Cells.Add(new BoxCell(CategorieNeufBox.ADevelopper, "À développer", _members[CategorieNeufBox.ADevelopper].Count));

        Cells.Add(new BoxCell(CategorieNeufBox.Expert, "Expert", _members[CategorieNeufBox.Expert].Count));
        Cells.Add(new BoxCell(CategorieNeufBox.AAccompagner, "À accompagner", _members[CategorieNeufBox.AAccompagner].Count));
        Cells.Add(new BoxCell(CategorieNeufBox.ASurveiller, "À surveiller", _members[CategorieNeufBox.ASurveiller].Count));
    }

    public List<Collaborateur> GetMembers(CategorieNeufBox category)
    {
        return _members.TryGetValue(category, out var list) ? list : new List<Collaborateur>();
    }
}
