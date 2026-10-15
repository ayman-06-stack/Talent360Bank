using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using OfficeOpenXml;

namespace Talent360.Infrastructure.Import;

/// <summary>
/// Service de prévisualisation d'import : lit un fichier CSV ou Excel
/// et retourne un ImportPreviewData SANS toucher à la base de données.
/// </summary>
public class ImportPreviewService
{
    private readonly AppDbContext _context;

    public ImportPreviewService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Génère un aperçu à partir d'un fichier CSV.
    /// </summary>
    public ImportPreviewData PrevisualiserCsv(string cheminFichier, string nomSource,
        string colonneMatricule = "Matricule")
    {
        var preview = new ImportPreviewData { NomSource = nomSource };

        using var reader = new StreamReader(cheminFichier);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            Delimiter = ";",
            HasHeaderRecord = true
        });

        csv.Read();
        csv.ReadHeader();
        preview.Colonnes = csv.HeaderRecord?.ToList() ?? new();

        var toutesLignes = new List<string[]>();
        while (csv.Read())
        {
            var row = new string[preview.Colonnes.Count];
            for (int i = 0; i < preview.Colonnes.Count; i++)
                row[i] = csv.GetField(i) ?? "";
            toutesLignes.Add(row);
        }

        preview.TotalLignes = toutesLignes.Count;
        preview.AperçuLignes = toutesLignes.Take(5).ToList();

        // Extraire les matricules du fichier
        int idxMatricule = preview.Colonnes
            .FindIndex(c => c.Equals(colonneMatricule, StringComparison.OrdinalIgnoreCase));

        if (idxMatricule >= 0)
        {
            preview.MatriculesFichier = toutesLignes
                .Select(r => r[idxMatricule]?.Trim() ?? "")
                .Where(m => !string.IsNullOrEmpty(m))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        CompterNouveauxEtExistants(preview, nomSource);
        return preview;
    }

    /// <summary>
    /// Génère un aperçu à partir d'un fichier Excel.
    /// </summary>
    public ImportPreviewData PrevisualiserExcel(string cheminFichier, string nomSource,
        string colonneMatricule = "Matricule")
    {
        var preview = new ImportPreviewData { NomSource = nomSource };

        ExcelPackage.License.SetNonCommercialOrganization("Talent360Bank");
        using var package = new ExcelPackage(new FileInfo(cheminFichier));
        var ws = package.Workbook.Worksheets[0];

        // En-têtes
        for (int col = 1; col <= ws.Dimension.End.Column; col++)
        {
            var val = ws.Cells[1, col].Text?.Trim() ?? "";
            preview.Colonnes.Add(val);
        }

        // Toutes les lignes
        var toutesLignes = new List<string[]>();
        for (int row = 2; row <= ws.Dimension.End.Row; row++)
        {
            var rowData = new string[preview.Colonnes.Count];
            for (int col = 1; col <= preview.Colonnes.Count; col++)
                rowData[col - 1] = ws.Cells[row, col].Text?.Trim() ?? "";
            toutesLignes.Add(rowData);
        }

        preview.TotalLignes = toutesLignes.Count;
        preview.AperçuLignes = toutesLignes.Take(5).ToList();

        // Extraire les matricules
        int idxMatricule = preview.Colonnes
            .FindIndex(c => c.Equals(colonneMatricule, StringComparison.OrdinalIgnoreCase));

        if (idxMatricule >= 0)
        {
            preview.MatriculesFichier = toutesLignes
                .Select(r => r[idxMatricule]?.Trim() ?? "")
                .Where(m => !string.IsNullOrEmpty(m))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        CompterNouveauxEtExistants(preview, nomSource);
        return preview;
    }

    /// <summary>
    /// Compte les matricules nouveaux vs existants selon la source.
    /// </summary>
    private void CompterNouveauxEtExistants(ImportPreviewData preview, string nomSource)
    {
        if (!preview.MatriculesFichier.Any()) return;

        // Pour SIRH, on compare avec la table Collaborateurs
        // Pour les autres sources, on compare aussi avec Collaborateurs (le matricule doit exister)
        var matriculesEnBase = _context.Collaborateurs
            .Select(c => c.Matricule)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Pour les sources d'import (non-SIRH), on vérifie aussi si des données existent déjà
        // dans la table d'import correspondante
        HashSet<string> matriculesImportExistants;

        if (nomSource.Contains("SIRH"))
        {
            matriculesImportExistants = matriculesEnBase;
        }
        else if (nomSource.Contains("Auto-éval"))
        {
            matriculesImportExistants = _context.ImportAutoEvaluations
                .Select(a => a.Matricule).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        else if (nomSource.Contains("Manager"))
        {
            matriculesImportExistants = _context.ImportEvaluationManagers
                .Select(e => e.Matricule).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        else if (nomSource.Contains("Potentiel"))
        {
            matriculesImportExistants = _context.ImportPotentiels
                .Select(p => p.Matricule).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        else if (nomSource.Contains("Formation"))
        {
            matriculesImportExistants = _context.ImportFormations
                .Select(f => f.Matricule).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        else if (nomSource.Contains("Mobilité"))
        {
            matriculesImportExistants = _context.ImportMobilites
                .Select(m => m.Matricule).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        else if (nomSource.Contains("Succession"))
        {
            // Succession uses PosteCritiqueTitre+MatriculeSuccesseur, but Matricule column is MatriculeSuccesseur
            matriculesImportExistants = _context.ImportSuccessions
                .Select(s => s.MatriculeSuccesseur).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        else if (nomSource.Contains("Carrière"))
        {
            matriculesImportExistants = _context.ImportCarrieres
                .Select(c => c.Matricule).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        else if (nomSource.Contains("Reconnaissance"))
        {
            matriculesImportExistants = _context.ImportReconnaissances
                .Select(r => r.Matricule).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        else if (nomSource.Contains("Leadership"))
        {
            matriculesImportExistants = _context.ImportLeaderships
                .Select(l => l.Matricule).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        else if (nomSource.Contains("Engagement"))
        {
            matriculesImportExistants = _context.ImportEngagements
                .Select(e => e.Matricule).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        else if (nomSource.Contains("Vigilance"))
        {
            matriculesImportExistants = _context.ImportVigilances
                .Select(v => v.Matricule).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        else
        {
            matriculesImportExistants = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        preview.MatriculesMisAJour = preview.MatriculesFichier
            .Count(m => matriculesImportExistants.Contains(m));
        preview.NouveauxMatricules = preview.MatriculesFichier.Count - preview.MatriculesMisAJour;
    }
}
