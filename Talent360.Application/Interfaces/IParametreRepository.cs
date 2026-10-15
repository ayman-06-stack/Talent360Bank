namespace Talent360.Application.Interfaces;

/// <summary>
/// Abstraction pour la lecture des paramètres (seuils, pondérations).
/// Permet de découpler CalculEngine de l'accès direct à la base de données.
/// </summary>
public interface IParametreRepository
{
    /// <summary>
    /// Retourne la valeur d'un paramètre par sa clé, ou la valeur par défaut si absent.
    /// </summary>
    string Obtenir(string cle, string valeurParDefaut = "");

    /// <summary>
    /// Retourne la valeur d'un paramètre sous forme de double.
    /// </summary>
    double ObtenirDouble(string cle, double valeurParDefaut = 0);

    /// <summary>
    /// Retourne la valeur d'un paramètre sous forme d'entier.
    /// </summary>
    int ObtenirInt(string cle, int valeurParDefaut = 0);
}
