using System.Globalization;
using CsvHelper.TypeConversion;
using OfficeOpenXml;
using Talent360.Domain.Entities;
using Talent360.Infrastructure;

namespace Talent360.Infrastructure.Import;

/// <summary>
/// Service d'import Excel (.xlsx) pour les 4 sources de données.
/// Mêmes colonnes attendues que le CSV (ligne 1 = en-têtes).
/// Utilise EPPlus pour la lecture.
/// </summary>
public class ExcelImportService
{
    private readonly AppDbContext _context;

    public ExcelImportService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Importe le fichier SIRH (collaborateurs).
    /// Colonnes : Matricule, Nom, Prenom, Direction, Poste, Grade, DateEntree
    /// </summary>
    public int ImporterSirh(string cheminFichier)
    {
        ExcelPackage.License.SetNonCommercialOrganization("Talent360Bank");
        using var package = new ExcelPackage(new FileInfo(cheminFichier));
        var ws = package.Workbook.Worksheets[0];
        var headers = LireEnTetes(ws);
        int count = 0;

        for (int row = 2; row <= ws.Dimension.End.Row; row++)
        {
            var matricule = CellStr(ws, row, headers, "Matricule");
            if (string.IsNullOrWhiteSpace(matricule)) continue;

            var existing = _context.Collaborateurs.FirstOrDefault(c => c.Matricule == matricule);
            if (existing != null)
            {
                existing.Nom = CellStr(ws, row, headers, "Nom");
                existing.Prenom = CellStr(ws, row, headers, "Prenom");
                existing.Direction = CellStr(ws, row, headers, "Direction");
                existing.Poste = CellStr(ws, row, headers, "Poste");
                existing.Grade = CellStr(ws, row, headers, "Grade");
                existing.SourceDonnees = SourceDonnees.ImportReel;
                if (TryParseDateFr(CellStr(ws, row, headers, "DateEntree"), out var dt))
                    existing.DateEntree = dt;
            }
            else
            {
                _context.Collaborateurs.Add(new Collaborateur
                {
                    Matricule = matricule,
                    Nom = CellStr(ws, row, headers, "Nom"),
                    Prenom = CellStr(ws, row, headers, "Prenom"),
                    Direction = CellStr(ws, row, headers, "Direction"),
                    Poste = CellStr(ws, row, headers, "Poste"),
                    Grade = CellStr(ws, row, headers, "Grade"),
                    SourceDonnees = SourceDonnees.ImportReel,
                    DateEntree = TryParseDateFr(CellStr(ws, row, headers, "DateEntree"), out var dt2) ? dt2 : DateTime.Today
                });
            }
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    /// <summary>
    /// Importe les auto-évaluations.
    /// Colonnes : Matricule, DateEvaluation, ScorePerformance, Competence_1, Note_1, Competence_2, Note_2 ...
    /// </summary>
    public int ImporterAutoEvaluation(string cheminFichier)
    {
        ExcelPackage.License.SetNonCommercialOrganization("Talent360Bank");
        using var package = new ExcelPackage(new FileInfo(cheminFichier));
        var ws = package.Workbook.Worksheets[0];
        var headers = LireEnTetes(ws);
        int count = 0;

        for (int row = 2; row <= ws.Dimension.End.Row; row++)
        {
            var matricule = CellStr(ws, row, headers, "Matricule");
            if (string.IsNullOrWhiteSpace(matricule)) continue;

            TryParseDateFr(CellStr(ws, row, headers, "DateEvaluation"), out var dateEval);

            var autoEval = new ImportAutoEvaluation
            {
                Matricule = matricule,
                DateEvaluation = dateEval,
                ScorePerformanceAutoEval = CellDouble(ws, row, headers, "ScorePerformance")
            };

            // Paires Competence_N / Note_N
            for (int i = 1; i <= 50; i++)
            {
                var compKey = $"Competence_{i}";
                var noteKey = $"Note_{i}";
                if (!headers.ContainsKey(compKey) || !headers.ContainsKey(noteKey)) break;

                var compNom = CellStr(ws, row, headers, compKey);
                if (string.IsNullOrWhiteSpace(compNom)) break;

                autoEval.Competences.Add(new CompetenceNote
                {
                    Nom = compNom.Trim(),
                    Note = CellDouble(ws, row, headers, noteKey)
                });
            }

            _context.ImportAutoEvaluations.Add(autoEval);
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    /// <summary>
    /// Importe les évaluations manager.
    /// Colonnes : Matricule, DateEvaluation, ScorePerformanceManager
    /// </summary>
    public int ImporterEvaluationManager(string cheminFichier)
    {
        ExcelPackage.License.SetNonCommercialOrganization("Talent360Bank");
        using var package = new ExcelPackage(new FileInfo(cheminFichier));
        var ws = package.Workbook.Worksheets[0];
        var headers = LireEnTetes(ws);
        int count = 0;

        for (int row = 2; row <= ws.Dimension.End.Row; row++)
        {
            var matricule = CellStr(ws, row, headers, "Matricule");
            if (string.IsNullOrWhiteSpace(matricule)) continue;

            TryParseDateFr(CellStr(ws, row, headers, "DateEvaluation"), out var dateEval);
            var score = CellDouble(ws, row, headers, "ScorePerformanceManager");

            var existing = _context.ImportEvaluationManagers
                .FirstOrDefault(e => e.Matricule == matricule);
            if (existing != null)
            {
                existing.DateEvaluation = dateEval;
                existing.ScorePerformanceManager = score;
            }
            else
            {
                _context.ImportEvaluationManagers.Add(new ImportEvaluationManager
                {
                    Matricule = matricule,
                    DateEvaluation = dateEval,
                    ScorePerformanceManager = score
                });
            }
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    /// <summary>
    /// Importe les scores d'engagement.
    /// Colonnes : Matricule, DateReponse, Management, Reconnaissance, Formation, EvolutionPro, ConditionsTravail, Communication
    /// </summary>
    public int ImporterEngagement(string cheminFichier)
    {
        ExcelPackage.License.SetNonCommercialOrganization("Talent360Bank");
        using var package = new ExcelPackage(new FileInfo(cheminFichier));
        var ws = package.Workbook.Worksheets[0];
        var headers = LireEnTetes(ws);
        int count = 0;

        for (int row = 2; row <= ws.Dimension.End.Row; row++)
        {
            var matricule = CellStr(ws, row, headers, "Matricule");
            if (string.IsNullOrWhiteSpace(matricule)) continue;

            TryParseDateFr(CellStr(ws, row, headers, "DateReponse"), out var dateReponse);

            var existing = _context.ImportEngagements
                .FirstOrDefault(e => e.Matricule == matricule);
            if (existing != null)
            {
                existing.DateReponse = dateReponse;
                existing.Management = CellDouble(ws, row, headers, "Management");
                existing.Reconnaissance = CellDouble(ws, row, headers, "Reconnaissance");
                existing.Formation = CellDouble(ws, row, headers, "Formation");
                existing.EvolutionPro = CellDouble(ws, row, headers, "EvolutionPro");
                existing.ConditionsTravail = CellDouble(ws, row, headers, "ConditionsTravail");
                existing.Communication = CellDouble(ws, row, headers, "Communication");
            }
            else
            {
                _context.ImportEngagements.Add(new ImportEngagement
                {
                    Matricule = matricule,
                    DateReponse = dateReponse,
                    Management = CellDouble(ws, row, headers, "Management"),
                    Reconnaissance = CellDouble(ws, row, headers, "Reconnaissance"),
                    Formation = CellDouble(ws, row, headers, "Formation"),
                    EvolutionPro = CellDouble(ws, row, headers, "EvolutionPro"),
                    ConditionsTravail = CellDouble(ws, row, headers, "ConditionsTravail"),
                    Communication = CellDouble(ws, row, headers, "Communication")
                });
            }
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    // ══════════════════════════════════════════════════════
    //  NOUVELLES SOURCES — Étape 3
    // ══════════════════════════════════════════════════════

    /// <summary>Importe les évaluations de potentiel (04_POTENTIEL).</summary>
    public int ImporterPotentiel(string cheminFichier)
    {
        ExcelPackage.License.SetNonCommercialOrganization("Talent360Bank");
        using var package = new ExcelPackage(new FileInfo(cheminFichier));
        var ws = package.Workbook.Worksheets[0];
        var headers = LireEnTetes(ws);
        int count = 0;

        for (int row = 2; row <= ws.Dimension.End.Row; row++)
        {
            var matricule = CellStr(ws, row, headers, "Matricule");
            if (string.IsNullOrWhiteSpace(matricule)) continue;

            TryParseDateFr(CellStr(ws, row, headers, "DateEvaluation"), out var dateEval);

            var existing = _context.ImportPotentiels.FirstOrDefault(p => p.Matricule == matricule);
            if (existing != null)
            {
                existing.DateEvaluation = dateEval;
                existing.AgiliteApprentissage = CellDouble(ws, row, headers, "AgiliteApprentissage");
                existing.Leadership = CellDouble(ws, row, headers, "Leadership");
                existing.Adaptabilite = CellDouble(ws, row, headers, "Adaptabilite");
                existing.GestionComplexite = CellDouble(ws, row, headers, "GestionComplexite");
                existing.Mobilite = CellDouble(ws, row, headers, "Mobilite");
                existing.VisionStrategique = CellDouble(ws, row, headers, "VisionStrategique");
                existing.Autonomie = CellDouble(ws, row, headers, "Autonomie");
            }
            else
            {
                _context.ImportPotentiels.Add(new ImportPotentiel
                {
                    Matricule = matricule,
                    DateEvaluation = dateEval,
                    AgiliteApprentissage = CellDouble(ws, row, headers, "AgiliteApprentissage"),
                    Leadership = CellDouble(ws, row, headers, "Leadership"),
                    Adaptabilite = CellDouble(ws, row, headers, "Adaptabilite"),
                    GestionComplexite = CellDouble(ws, row, headers, "GestionComplexite"),
                    Mobilite = CellDouble(ws, row, headers, "Mobilite"),
                    VisionStrategique = CellDouble(ws, row, headers, "VisionStrategique"),
                    Autonomie = CellDouble(ws, row, headers, "Autonomie")
                });
            }
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    /// <summary>Importe les actions de formation (05_FORMATION).</summary>
    public int ImporterFormation(string cheminFichier)
    {
        ExcelPackage.License.SetNonCommercialOrganization("Talent360Bank");
        using var package = new ExcelPackage(new FileInfo(cheminFichier));
        var ws = package.Workbook.Worksheets[0];
        var headers = LireEnTetes(ws);
        int count = 0;

        // Formation : delete-reinsert pour gérer les entrées multiples par matricule
        var matriculesVus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int row = 2; row <= ws.Dimension.End.Row; row++)
        {
            var m = CellStr(ws, row, headers, "Matricule");
            if (!string.IsNullOrWhiteSpace(m)) matriculesVus.Add(m);
        }
        var anciennes = _context.ImportFormations
            .Where(f => matriculesVus.Contains(f.Matricule)).ToList();
        _context.ImportFormations.RemoveRange(anciennes);

        for (int row = 2; row <= ws.Dimension.End.Row; row++)
        {
            var matricule = CellStr(ws, row, headers, "Matricule");
            if (string.IsNullOrWhiteSpace(matricule)) continue;

            TryParseDateFr(CellStr(ws, row, headers, "DateFormation"), out var dateFrm);

            _context.ImportFormations.Add(new ImportFormation
            {
                Matricule = matricule,
                DateFormation = dateFrm,
                Intitule = CellStr(ws, row, headers, "Intitule"),
                Type = CellStr(ws, row, headers, "Type"),
                Statut = CellStr(ws, row, headers, "Statut"),
                DureeHeures = CellInt(ws, row, headers, "DureeHeures")
            });
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    /// <summary>Importe les demandes de mobilité (06_MOBILITE).</summary>
    public int ImporterMobilite(string cheminFichier)
    {
        ExcelPackage.License.SetNonCommercialOrganization("Talent360Bank");
        using var package = new ExcelPackage(new FileInfo(cheminFichier));
        var ws = package.Workbook.Worksheets[0];
        var headers = LireEnTetes(ws);
        int count = 0;

        for (int row = 2; row <= ws.Dimension.End.Row; row++)
        {
            var matricule = CellStr(ws, row, headers, "Matricule");
            if (string.IsNullOrWhiteSpace(matricule)) continue;

            TryParseDateFr(CellStr(ws, row, headers, "DateDemande"), out var dateDem);

            var existing = _context.ImportMobilites.FirstOrDefault(m => m.Matricule == matricule);
            if (existing != null)
            {
                existing.DateDemande = dateDem;
                existing.TypeMobilite = CellStr(ws, row, headers, "TypeMobilite");
                existing.Statut = CellStr(ws, row, headers, "Statut");
                existing.ScoreMobilite = CellDouble(ws, row, headers, "ScoreMobilite");
            }
            else
            {
                _context.ImportMobilites.Add(new ImportMobilite
                {
                    Matricule = matricule,
                    DateDemande = dateDem,
                    TypeMobilite = CellStr(ws, row, headers, "TypeMobilite"),
                    Statut = CellStr(ws, row, headers, "Statut"),
                    ScoreMobilite = CellDouble(ws, row, headers, "ScoreMobilite")
                });
            }
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    /// <summary>Importe les plans de succession (07_SUCCESSION).</summary>
    public int ImporterSuccession(string cheminFichier)
    {
        ExcelPackage.License.SetNonCommercialOrganization("Talent360Bank");
        using var package = new ExcelPackage(new FileInfo(cheminFichier));
        var ws = package.Workbook.Worksheets[0];
        var headers = LireEnTetes(ws);
        int count = 0;

        for (int row = 2; row <= ws.Dimension.End.Row; row++)
        {
            var poste = CellStr(ws, row, headers, "PosteCritiqueTitre");
            if (string.IsNullOrWhiteSpace(poste)) continue;

            TryParseDateFr(CellStr(ws, row, headers, "DateMiseAJour"), out var dateMaj);
            var matSucc = CellStr(ws, row, headers, "MatriculeSuccesseur");

            var existing = _context.ImportSuccessions
                .FirstOrDefault(s => s.MatriculeSuccesseur == matSucc && s.PosteCritiqueTitre == poste);
            if (existing != null)
            {
                existing.Readiness = CellStr(ws, row, headers, "Readiness");
                existing.Priorite = CellInt(ws, row, headers, "Priorite");
                existing.DateMiseAJour = dateMaj;
            }
            else
            {
                _context.ImportSuccessions.Add(new ImportSuccession
                {
                    PosteCritiqueTitre = poste,
                    MatriculeSuccesseur = matSucc,
                    Readiness = CellStr(ws, row, headers, "Readiness"),
                    Priorite = CellInt(ws, row, headers, "Priorite"),
                    DateMiseAJour = dateMaj
                });
            }
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    /// <summary>Importe les mouvements de carrière (08_CARRIERE).</summary>
    public int ImporterCarriere(string cheminFichier)
    {
        ExcelPackage.License.SetNonCommercialOrganization("Talent360Bank");
        using var package = new ExcelPackage(new FileInfo(cheminFichier));
        var ws = package.Workbook.Worksheets[0];
        var headers = LireEnTetes(ws);
        int count = 0;

        // Carrière : delete-reinsert (multiples mouvements par matricule)
        var matriculesVus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int row = 2; row <= ws.Dimension.End.Row; row++)
        {
            var m = CellStr(ws, row, headers, "Matricule");
            if (!string.IsNullOrWhiteSpace(m)) matriculesVus.Add(m);
        }
        var anciens = _context.ImportCarrieres
            .Where(c => matriculesVus.Contains(c.Matricule)).ToList();
        _context.ImportCarrieres.RemoveRange(anciens);

        for (int row = 2; row <= ws.Dimension.End.Row; row++)
        {
            var matricule = CellStr(ws, row, headers, "Matricule");
            if (string.IsNullOrWhiteSpace(matricule)) continue;

            TryParseDateFr(CellStr(ws, row, headers, "DateMouvement"), out var dateMvt);

            _context.ImportCarrieres.Add(new ImportCarriere
            {
                Matricule = matricule,
                DateMouvement = dateMvt,
                TypeMouvement = CellStr(ws, row, headers, "TypeMouvement"),
                PosteAvant = CellStr(ws, row, headers, "PosteAvant"),
                PosteApres = CellStr(ws, row, headers, "PosteApres"),
                DirectionAvant = CellStr(ws, row, headers, "DirectionAvant"),
                DirectionApres = CellStr(ws, row, headers, "DirectionApres")
            });
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    /// <summary>Importe les événements de reconnaissance (09_RECONNAISSANCE).</summary>
    public int ImporterReconnaissance(string cheminFichier)
    {
        ExcelPackage.License.SetNonCommercialOrganization("Talent360Bank");
        using var package = new ExcelPackage(new FileInfo(cheminFichier));
        var ws = package.Workbook.Worksheets[0];
        var headers = LireEnTetes(ws);
        int count = 0;

        // Reconnaissance : delete-reinsert
        var matriculesVus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int row = 2; row <= ws.Dimension.End.Row; row++)
        {
            var m = CellStr(ws, row, headers, "Matricule");
            if (!string.IsNullOrWhiteSpace(m)) matriculesVus.Add(m);
        }
        var anciens = _context.ImportReconnaissances
            .Where(r => matriculesVus.Contains(r.Matricule)).ToList();
        _context.ImportReconnaissances.RemoveRange(anciens);

        for (int row = 2; row <= ws.Dimension.End.Row; row++)
        {
            var matricule = CellStr(ws, row, headers, "Matricule");
            if (string.IsNullOrWhiteSpace(matricule)) continue;

            TryParseDateFr(CellStr(ws, row, headers, "DateReconnaissance"), out var dateReco);

            _context.ImportReconnaissances.Add(new ImportReconnaissance
            {
                Matricule = matricule,
                DateReconnaissance = dateReco,
                Type = CellStr(ws, row, headers, "Type"),
                Description = CellStr(ws, row, headers, "Description")
            });
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    /// <summary>Importe les scores de leadership 360° (10_LEADERSHIP).</summary>
    public int ImporterLeadership(string cheminFichier)
    {
        ExcelPackage.License.SetNonCommercialOrganization("Talent360Bank");
        using var package = new ExcelPackage(new FileInfo(cheminFichier));
        var ws = package.Workbook.Worksheets[0];
        var headers = LireEnTetes(ws);
        int count = 0;

        for (int row = 2; row <= ws.Dimension.End.Row; row++)
        {
            var matricule = CellStr(ws, row, headers, "Matricule");
            if (string.IsNullOrWhiteSpace(matricule)) continue;

            TryParseDateFr(CellStr(ws, row, headers, "DateEvaluation"), out var dateEval);

            var existing = _context.ImportLeaderships.FirstOrDefault(l => l.Matricule == matricule);
            if (existing != null)
            {
                existing.DateEvaluation = dateEval;
                existing.ScoreLeadership = CellDouble(ws, row, headers, "ScoreLeadership");
                existing.ScoreEquipe = CellDouble(ws, row, headers, "ScoreEquipe");
                existing.ScorePairs = CellDouble(ws, row, headers, "ScorePairs");
                existing.ScoreHierarchie = CellDouble(ws, row, headers, "ScoreHierarchie");
            }
            else
            {
                _context.ImportLeaderships.Add(new ImportLeadership
                {
                    Matricule = matricule,
                    DateEvaluation = dateEval,
                    ScoreLeadership = CellDouble(ws, row, headers, "ScoreLeadership"),
                    ScoreEquipe = CellDouble(ws, row, headers, "ScoreEquipe"),
                    ScorePairs = CellDouble(ws, row, headers, "ScorePairs"),
                    ScoreHierarchie = CellDouble(ws, row, headers, "ScoreHierarchie")
                });
            }
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    /// <summary>Importe les données de vigilance (12_VIGILANCE).</summary>
    public int ImporterVigilance(string cheminFichier)
    {
        ExcelPackage.License.SetNonCommercialOrganization("Talent360Bank");
        using var package = new ExcelPackage(new FileInfo(cheminFichier));
        var ws = package.Workbook.Worksheets[0];
        var headers = LireEnTetes(ws);
        int count = 0;

        for (int row = 2; row <= ws.Dimension.End.Row; row++)
        {
            var matricule = CellStr(ws, row, headers, "Matricule");
            if (string.IsNullOrWhiteSpace(matricule)) continue;

            TryParseDateFr(CellStr(ws, row, headers, "DateEvaluation"), out var dateEval);

            var existing = _context.ImportVigilances.FirstOrDefault(v => v.Matricule == matricule);
            if (existing != null)
            {
                existing.DateEvaluation = dateEval;
                existing.BaissePerformance = CellBool(ws, row, headers, "BaissePerformance");
                existing.ScorePerformanceN1 = CellDouble(ws, row, headers, "ScorePerformanceN1");
                existing.Commentaire = CellStr(ws, row, headers, "Commentaire");
            }
            else
            {
                _context.ImportVigilances.Add(new ImportVigilance
                {
                    Matricule = matricule,
                    DateEvaluation = dateEval,
                    BaissePerformance = CellBool(ws, row, headers, "BaissePerformance"),
                    ScorePerformanceN1 = CellDouble(ws, row, headers, "ScorePerformanceN1"),
                    Commentaire = CellStr(ws, row, headers, "Commentaire")
                });
            }
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    // ── Helpers ──

    /// <summary>Lit la ligne 1 en tant qu'en-têtes → dictionnaire nom→colonne.</summary>
    private static Dictionary<string, int> LireEnTetes(ExcelWorksheet ws)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int col = 1; col <= ws.Dimension.End.Column; col++)
        {
            var val = ws.Cells[1, col].Text?.Trim();
            if (!string.IsNullOrEmpty(val) && !map.ContainsKey(val))
                map[val] = col;
        }
        return map;
    }

    private static string CellStr(ExcelWorksheet ws, int row, Dictionary<string, int> headers, string key)
    {
        return headers.TryGetValue(key, out var col) ? (ws.Cells[row, col].Text?.Trim() ?? "") : "";
    }

    private static double CellDouble(ExcelWorksheet ws, int row, Dictionary<string, int> headers, string key)
    {
        var str = CellStr(ws, row, headers, key);
        return double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0;
    }

    private static int CellInt(ExcelWorksheet ws, int row, Dictionary<string, int> headers, string key)
    {
        var str = CellStr(ws, row, headers, key);
        return int.TryParse(str, out var val) ? val : 0;
    }

    private static bool CellBool(ExcelWorksheet ws, int row, Dictionary<string, int> headers, string key)
    {
        var str = CellStr(ws, row, headers, key).ToLowerInvariant();
        return str is "true" or "1" or "oui" or "vrai";
    }

    private static bool TryParseDateFr(string input, out DateTime value)
    {
        return DateTime.TryParse(input, CultureInfo.GetCultureInfo("fr-FR"), DateTimeStyles.None, out value);
    }
}

