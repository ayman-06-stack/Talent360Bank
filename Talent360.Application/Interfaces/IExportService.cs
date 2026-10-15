namespace Talent360.Application.Interfaces;

/// <summary>
/// Interface pour l'export de données vers des fichiers.
/// </summary>
public interface IExportService
{
    void ExporterCollaborateurs(string cheminFichier);
}
