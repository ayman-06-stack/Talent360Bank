using OfficeOpenXml;
using Talent360.Application.Enums;
using Talent360.Application.Interfaces;

namespace Talent360.Infrastructure.Export;

/// <summary>
/// Service d'export Excel via EPPlus.
/// </summary>
public class ExcelExportService : IExportService
{
    private readonly ICollaborateurRepository _collaborateurRepo;
    private readonly ICalculEngine _calculEngine;

    public ExcelExportService(ICollaborateurRepository collaborateurRepo, ICalculEngine calculEngine)
    {
        _collaborateurRepo = collaborateurRepo;
        _calculEngine = calculEngine;
    }

    public void ExporterCollaborateurs(string cheminFichier)
    {
        ExcelPackage.License.SetNonCommercialOrganization("Talent360Bank");

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Collaborateurs");

        // En-têtes
        string[] headers = { "Matricule", "Nom", "Prénom", "Direction", "Poste",
            "Grade", "Performance", "Potentiel", "Catégorie 9-Box", "Statut Talent",
            "Vivier", "Poste cible", "Matching", "Readiness" };

        for (int i = 0; i < headers.Length; i++)
            ws.Cells[1, i + 1].Value = headers[i];

        // Style en-têtes
        using (var range = ws.Cells[1, 1, 1, headers.Length])
        {
            range.Style.Font.Bold = true;
        }

        // Données
        var collaborateurs = _collaborateurRepo.ObtenirTous();
        int row = 2;
        foreach (var c in collaborateurs)
        {
            var categorie9Box = _calculEngine.CalculerNeufBox(c.ScorePerformance, c.ScorePotentiel);
            string statutTalent = c.TalentValide ? "Validé"
                : c.TalentPropose ? "Proposé" : "—";

            ws.Cells[row, 1].Value = c.Matricule;
            ws.Cells[row, 2].Value = c.Nom;
            ws.Cells[row, 3].Value = c.Prenom;
            ws.Cells[row, 4].Value = c.Direction;
            ws.Cells[row, 5].Value = c.Poste;
            ws.Cells[row, 6].Value = c.Grade;
            ws.Cells[row, 7].Value = c.ScorePerformance;
            ws.Cells[row, 8].Value = c.ScorePotentiel;
            ws.Cells[row, 9].Value = categorie9Box.ToString();
            ws.Cells[row, 10].Value = statutTalent;
            ws.Cells[row, 11].Value = c.Vivier ?? "—";
            ws.Cells[row, 12].Value = c.PosteCible ?? "—";
            ws.Cells[row, 13].Value = c.Matching.HasValue ? c.Matching.Value : 0;
            ws.Cells[row, 14].Value = c.Readiness ?? "—";
            row++;
        }

        // Auto-fit colonnes
        ws.Cells[ws.Dimension.Address].AutoFitColumns();

        package.SaveAs(new FileInfo(cheminFichier));
    }
}
