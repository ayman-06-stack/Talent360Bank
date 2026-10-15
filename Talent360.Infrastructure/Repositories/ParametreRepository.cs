using System.Globalization;
using Talent360.Application.Interfaces;

namespace Talent360.Infrastructure.Repositories;

/// <summary>
/// Lecture des paramètres depuis la base de données via AppDbContext.
/// </summary>
public class ParametreRepository : IParametreRepository
{
    private readonly AppDbContext _context;

    public ParametreRepository(AppDbContext context)
    {
        _context = context;
    }

    public string Obtenir(string cle, string valeurParDefaut = "")
    {
        var parametre = _context.Parametres.FirstOrDefault(p => p.Cle == cle);
        return parametre?.Valeur ?? valeurParDefaut;
    }

    public double ObtenirDouble(string cle, double valeurParDefaut = 0)
    {
        var valeur = Obtenir(cle);
        return string.IsNullOrEmpty(valeur)
            ? valeurParDefaut
            : double.TryParse(valeur, CultureInfo.InvariantCulture, out var result) ? result : valeurParDefaut;
    }

    public int ObtenirInt(string cle, int valeurParDefaut = 0)
    {
        var valeur = Obtenir(cle);
        return string.IsNullOrEmpty(valeur)
            ? valeurParDefaut
            : int.TryParse(valeur, out var result) ? result : valeurParDefaut;
    }
}
