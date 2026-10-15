using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Talent360.Domain.Entities;
using Talent360.Infrastructure;

namespace Talent360.Infrastructure.Import;

/// <summary>
/// Service d'import CSV pour les 4 sources de données :
/// SIRH (collaborateurs), auto-évaluations, évaluations manager, engagement.
/// Utilise CsvHelper pour le parsing.
/// </summary>
public class CsvImportService
{
    private static readonly CultureInfo FrenchCulture = CultureInfo.GetCultureInfo("fr-FR");
    private readonly AppDbContext _context;

    public CsvImportService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Importe le fichier SIRH (collaborateurs).
    /// Colonnes attendues : Matricule, Nom, Prenom, Direction, Poste, Grade, DateEntree
    /// Mise à jour si le matricule existe déjà, insertion sinon.
    /// </summary>
    public int ImporterSirh(string cheminFichier)
    {
        using var reader = new StreamReader(cheminFichier);
        using var csv = new CsvReader(reader, CsvConfig());
        ConfigureDateOptions(csv);

        var records = csv.GetRecords<SirhRecord>().ToList();
        int count = 0;

        foreach (var r in records)
        {
            var existing = _context.Collaborateurs.FirstOrDefault(c => c.Matricule == r.Matricule);
            if (existing != null)
            {
                existing.Nom = r.Nom;
                existing.Prenom = r.Prenom;
                existing.Direction = r.Direction;
                existing.Poste = r.Poste;
                existing.Grade = r.Grade;
                existing.SourceDonnees = SourceDonnees.ImportReel;
                if (TryParseDateFr(r.DateEntree, out var dt))
                    existing.DateEntree = dt;
            }
            else
            {
                var collab = new Collaborateur
                {
                    Matricule = r.Matricule,
                    Nom = r.Nom,
                    Prenom = r.Prenom,
                    Direction = r.Direction,
                    Poste = r.Poste,
                    Grade = r.Grade,
                    SourceDonnees = SourceDonnees.ImportReel,
                    DateEntree = TryParseDateFr(r.DateEntree, out var dt2) ? dt2 : DateTime.Today
                };
                _context.Collaborateurs.Add(collab);
            }
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    /// <summary>
    /// Importe les auto-évaluations.
    /// Colonnes attendues : Matricule, DateEvaluation, ScorePerformance, puis paires Competence_N / Note_N
    /// </summary>
    public int ImporterAutoEvaluation(string cheminFichier)
    {
        using var reader = new StreamReader(cheminFichier);
        using var csv = new CsvReader(reader, CsvConfig());
        ConfigureDateOptions(csv);

        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? Array.Empty<string>();
        int count = 0;

        while (csv.Read())
        {
            var matricule = csv.GetField<string>("Matricule") ?? "";
            var dateStr = csv.GetField<string>("DateEvaluation") ?? "";
            var scorePerfStr = csv.GetField<string>("ScorePerformance") ?? "0";

            TryParseDateFr(dateStr, out var dateEval);
            double.TryParse(scorePerfStr, NumberStyles.Any, FrenchCulture, out var scorePerf);

            var autoEval = new ImportAutoEvaluation
            {
                Matricule = matricule,
                DateEvaluation = dateEval,
                ScorePerformanceAutoEval = scorePerf
            };

            // Extraire les paires Competence_N / Note_N
            for (int i = 1; i <= 50; i++)
            {
                var compHeader = $"Competence_{i}";
                var noteHeader = $"Note_{i}";

                if (!headers.Contains(compHeader) || !headers.Contains(noteHeader)) break;

                var compNom = csv.GetField<string>(compHeader);
                var noteStr = csv.GetField<string>(noteHeader);

                if (string.IsNullOrWhiteSpace(compNom)) break;

                double.TryParse(noteStr, NumberStyles.Any, FrenchCulture, out var note);
                autoEval.Competences.Add(new CompetenceNote
                {
                    Nom = compNom.Trim(),
                    Note = note
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
    /// Colonnes attendues : Matricule, DateEvaluation, ScorePerformanceManager
    /// </summary>
    public int ImporterEvaluationManager(string cheminFichier)
    {
        using var reader = new StreamReader(cheminFichier);
        using var csv = new CsvReader(reader, CsvConfig());
        ConfigureDateOptions(csv);

        var records = csv.GetRecords<EvalManagerRecord>().ToList();
        int count = 0;

        foreach (var r in records)
        {
            var existing = _context.ImportEvaluationManagers
                .FirstOrDefault(e => e.Matricule == r.Matricule);
            if (existing != null)
            {
                existing.DateEvaluation = r.DateEvaluation;
                existing.ScorePerformanceManager = r.ScorePerformanceManager;
            }
            else
            {
                _context.ImportEvaluationManagers.Add(new ImportEvaluationManager
                {
                    Matricule = r.Matricule,
                    DateEvaluation = r.DateEvaluation,
                    ScorePerformanceManager = r.ScorePerformanceManager
                });
            }
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    /// <summary>
    /// Importe les scores d'engagement.
    /// Colonnes attendues : Matricule, DateReponse, Management, Reconnaissance, Formation, EvolutionPro, ConditionsTravail, Communication
    /// </summary>
    public int ImporterEngagement(string cheminFichier)
    {
        using var reader = new StreamReader(cheminFichier);
        using var csv = new CsvReader(reader, CsvConfig());
        ConfigureDateOptions(csv);

        var records = csv.GetRecords<EngagementRecord>().ToList();
        int count = 0;

        foreach (var r in records)
        {
            var existing = _context.ImportEngagements
                .FirstOrDefault(e => e.Matricule == r.Matricule);
            if (existing != null)
            {
                existing.DateReponse = r.DateReponse;
                existing.Management = r.Management;
                existing.Reconnaissance = r.Reconnaissance;
                existing.Formation = r.Formation;
                existing.EvolutionPro = r.EvolutionPro;
                existing.ConditionsTravail = r.ConditionsTravail;
                existing.Communication = r.Communication;
            }
            else
            {
                _context.ImportEngagements.Add(new ImportEngagement
                {
                    Matricule = r.Matricule,
                    DateReponse = r.DateReponse,
                    Management = r.Management,
                    Reconnaissance = r.Reconnaissance,
                    Formation = r.Formation,
                    EvolutionPro = r.EvolutionPro,
                    ConditionsTravail = r.ConditionsTravail,
                    Communication = r.Communication
                });
            }
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    // ── Records internes pour CsvHelper ──

    private class SirhRecord
    {
        public string Matricule { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public string Poste { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string DateEntree { get; set; } = string.Empty;
    }

    private class EvalManagerRecord
    {
        public string Matricule { get; set; } = string.Empty;
        public DateTime DateEvaluation { get; set; }
        public double ScorePerformanceManager { get; set; }
    }

    private class EngagementRecord
    {
        public string Matricule { get; set; } = string.Empty;
        public DateTime DateReponse { get; set; }
        public double Management { get; set; }
        public double Reconnaissance { get; set; }
        public double Formation { get; set; }
        public double EvolutionPro { get; set; }
        public double ConditionsTravail { get; set; }
        public double Communication { get; set; }
    }

    // ══════════════════════════════════════════════════════
    //  NOUVELLES SOURCES — Étape 3
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// Importe les évaluations de potentiel (04_POTENTIEL).
    /// Colonnes : Matricule, DateEvaluation, AgiliteApprentissage, Leadership, Adaptabilite,
    ///            GestionComplexite, Mobilite, VisionStrategique, Autonomie
    /// </summary>
    public int ImporterPotentiel(string cheminFichier)
    {
        using var reader = new StreamReader(cheminFichier);
        using var csv = new CsvReader(reader, CsvConfig());
        ConfigureDateOptions(csv);

        var records = csv.GetRecords<PotentielRecord>().ToList();
        int count = 0;

        foreach (var r in records)
        {
            var existing = _context.ImportPotentiels
                .FirstOrDefault(p => p.Matricule == r.Matricule);
            if (existing != null)
            {
                existing.DateEvaluation = r.DateEvaluation;
                existing.AgiliteApprentissage = r.AgiliteApprentissage;
                existing.Leadership = r.Leadership;
                existing.Adaptabilite = r.Adaptabilite;
                existing.GestionComplexite = r.GestionComplexite;
                existing.Mobilite = r.Mobilite;
                existing.VisionStrategique = r.VisionStrategique;
                existing.Autonomie = r.Autonomie;
            }
            else
            {
                _context.ImportPotentiels.Add(new ImportPotentiel
                {
                    Matricule = r.Matricule,
                    DateEvaluation = r.DateEvaluation,
                    AgiliteApprentissage = r.AgiliteApprentissage,
                    Leadership = r.Leadership,
                    Adaptabilite = r.Adaptabilite,
                    GestionComplexite = r.GestionComplexite,
                    Mobilite = r.Mobilite,
                    VisionStrategique = r.VisionStrategique,
                    Autonomie = r.Autonomie
                });
            }
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    /// <summary>
    /// Importe les actions de formation (05_FORMATION).
    /// Colonnes : Matricule, DateFormation, Intitule, Type, Statut, DureeHeures
    /// </summary>
    public int ImporterFormation(string cheminFichier)
    {
        using var reader = new StreamReader(cheminFichier);
        using var csv = new CsvReader(reader, CsvConfig());
        ConfigureDateOptions(csv);

        var records = csv.GetRecords<FormationRecord>().ToList();
        int count = 0;

        // Formation : supprimer les anciens enregistrements du même matricule puis réinsérer
        var matriculesConcernes = records.Select(r => r.Matricule).Distinct().ToList();
        var anciennes = _context.ImportFormations
            .Where(f => matriculesConcernes.Contains(f.Matricule)).ToList();
        _context.ImportFormations.RemoveRange(anciennes);

        foreach (var r in records)
        {
            _context.ImportFormations.Add(new ImportFormation
            {
                Matricule = r.Matricule,
                DateFormation = r.DateFormation,
                Intitule = r.Intitule,
                Type = r.Type,
                Statut = r.Statut,
                DureeHeures = r.DureeHeures
            });
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    /// <summary>
    /// Importe les demandes de mobilité (06_MOBILITE).
    /// Colonnes : Matricule, DateDemande, TypeMobilite, Statut, ScoreMobilite
    /// </summary>
    public int ImporterMobilite(string cheminFichier)
    {
        using var reader = new StreamReader(cheminFichier);
        using var csv = new CsvReader(reader, CsvConfig());
        ConfigureDateOptions(csv);

        var records = csv.GetRecords<MobiliteRecord>().ToList();
        int count = 0;

        foreach (var r in records)
        {
            var existing = _context.ImportMobilites
                .FirstOrDefault(m => m.Matricule == r.Matricule);
            if (existing != null)
            {
                existing.DateDemande = r.DateDemande;
                existing.TypeMobilite = r.TypeMobilite;
                existing.Statut = r.Statut;
                existing.ScoreMobilite = r.ScoreMobilite;
            }
            else
            {
                _context.ImportMobilites.Add(new ImportMobilite
                {
                    Matricule = r.Matricule,
                    DateDemande = r.DateDemande,
                    TypeMobilite = r.TypeMobilite,
                    Statut = r.Statut,
                    ScoreMobilite = r.ScoreMobilite
                });
            }
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    /// <summary>
    /// Importe les plans de succession (07_SUCCESSION).
    /// Colonnes : PosteCritiqueTitre, MatriculeSuccesseur, Readiness, Priorite, DateMiseAJour
    /// </summary>
    public int ImporterSuccession(string cheminFichier)
    {
        using var reader = new StreamReader(cheminFichier);
        using var csv = new CsvReader(reader, CsvConfig());
        ConfigureDateOptions(csv);

        var records = csv.GetRecords<SuccessionRecord>().ToList();
        int count = 0;

        foreach (var r in records)
        {
            var existing = _context.ImportSuccessions
                .FirstOrDefault(s => s.MatriculeSuccesseur == r.MatriculeSuccesseur
                    && s.PosteCritiqueTitre == r.PosteCritiqueTitre);
            if (existing != null)
            {
                existing.Readiness = r.Readiness;
                existing.Priorite = r.Priorite;
                existing.DateMiseAJour = r.DateMiseAJour;
            }
            else
            {
                _context.ImportSuccessions.Add(new ImportSuccession
                {
                    PosteCritiqueTitre = r.PosteCritiqueTitre,
                    MatriculeSuccesseur = r.MatriculeSuccesseur,
                    Readiness = r.Readiness,
                    Priorite = r.Priorite,
                    DateMiseAJour = r.DateMiseAJour
                });
            }
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    /// <summary>
    /// Importe les mouvements de carrière (08_CARRIERE).
    /// Colonnes : Matricule, DateMouvement, TypeMouvement, PosteAvant, PosteApres, DirectionAvant, DirectionApres
    /// </summary>
    public int ImporterCarriere(string cheminFichier)
    {
        using var reader = new StreamReader(cheminFichier);
        using var csv = new CsvReader(reader, CsvConfig());
        ConfigureDateOptions(csv);

        var records = csv.GetRecords<CarriereRecord>().ToList();
        int count = 0;

        // Carrière : supprimer puis réinsérer (plusieurs mouvements par matricule)
        var matriculesConcernes = records.Select(r => r.Matricule).Distinct().ToList();
        var anciens = _context.ImportCarrieres
            .Where(c => matriculesConcernes.Contains(c.Matricule)).ToList();
        _context.ImportCarrieres.RemoveRange(anciens);

        foreach (var r in records)
        {
            _context.ImportCarrieres.Add(new ImportCarriere
            {
                Matricule = r.Matricule,
                DateMouvement = r.DateMouvement,
                TypeMouvement = r.TypeMouvement,
                PosteAvant = r.PosteAvant,
                PosteApres = r.PosteApres,
                DirectionAvant = r.DirectionAvant,
                DirectionApres = r.DirectionApres
            });
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    /// <summary>
    /// Importe les événements de reconnaissance (09_RECONNAISSANCE).
    /// Colonnes : Matricule, DateReconnaissance, Type, Description
    /// </summary>
    public int ImporterReconnaissance(string cheminFichier)
    {
        using var reader = new StreamReader(cheminFichier);
        using var csv = new CsvReader(reader, CsvConfig());
        ConfigureDateOptions(csv);

        var records = csv.GetRecords<ReconnaissanceRecord>().ToList();
        int count = 0;

        // Reconnaissance : supprimer puis réinsérer (plusieurs événements par matricule)
        var matriculesConcernes = records.Select(r => r.Matricule).Distinct().ToList();
        var anciens = _context.ImportReconnaissances
            .Where(r => matriculesConcernes.Contains(r.Matricule)).ToList();
        _context.ImportReconnaissances.RemoveRange(anciens);

        foreach (var r in records)
        {
            _context.ImportReconnaissances.Add(new ImportReconnaissance
            {
                Matricule = r.Matricule,
                DateReconnaissance = r.DateReconnaissance,
                Type = r.Type,
                Description = r.Description
            });
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    /// <summary>
    /// Importe les scores de leadership 360° (10_LEADERSHIP).
    /// Colonnes : Matricule, DateEvaluation, ScoreLeadership, ScoreEquipe, ScorePairs, ScoreHierarchie
    /// </summary>
    public int ImporterLeadership(string cheminFichier)
    {
        using var reader = new StreamReader(cheminFichier);
        using var csv = new CsvReader(reader, CsvConfig());
        ConfigureDateOptions(csv);

        var records = csv.GetRecords<LeadershipRecord>().ToList();
        int count = 0;

        foreach (var r in records)
        {
            var existing = _context.ImportLeaderships
                .FirstOrDefault(l => l.Matricule == r.Matricule);
            if (existing != null)
            {
                existing.DateEvaluation = r.DateEvaluation;
                existing.ScoreLeadership = r.ScoreLeadership;
                existing.ScoreEquipe = r.ScoreEquipe;
                existing.ScorePairs = r.ScorePairs;
                existing.ScoreHierarchie = r.ScoreHierarchie;
            }
            else
            {
                _context.ImportLeaderships.Add(new ImportLeadership
                {
                    Matricule = r.Matricule,
                    DateEvaluation = r.DateEvaluation,
                    ScoreLeadership = r.ScoreLeadership,
                    ScoreEquipe = r.ScoreEquipe,
                    ScorePairs = r.ScorePairs,
                    ScoreHierarchie = r.ScoreHierarchie
                });
            }
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    /// <summary>
    /// Importe les données de vigilance (12_VIGILANCE).
    /// Colonnes : Matricule, DateEvaluation, BaissePerformance, ScorePerformanceN1, Commentaire
    /// </summary>
    public int ImporterVigilance(string cheminFichier)
    {
        using var reader = new StreamReader(cheminFichier);
        using var csv = new CsvReader(reader, CsvConfig());
        ConfigureDateOptions(csv);

        var records = csv.GetRecords<VigilanceRecord>().ToList();
        int count = 0;

        foreach (var r in records)
        {
            var existing = _context.ImportVigilances
                .FirstOrDefault(v => v.Matricule == r.Matricule);
            if (existing != null)
            {
                existing.DateEvaluation = r.DateEvaluation;
                existing.BaissePerformance = r.BaissePerformance;
                existing.ScorePerformanceN1 = r.ScorePerformanceN1;
                existing.Commentaire = r.Commentaire;
            }
            else
            {
                _context.ImportVigilances.Add(new ImportVigilance
                {
                    Matricule = r.Matricule,
                    DateEvaluation = r.DateEvaluation,
                    BaissePerformance = r.BaissePerformance,
                    ScorePerformanceN1 = r.ScorePerformanceN1,
                    Commentaire = r.Commentaire
                });
            }
            count++;
        }

        _context.SaveChanges();
        return count;
    }

    // ── Helper : config CsvHelper commune ──
    private static CsvConfiguration CsvConfig() => new(FrenchCulture)
    {
        HeaderValidated = null,
        MissingFieldFound = null,
        Delimiter = ";",
        HasHeaderRecord = true
    };

    private static void ConfigureDateOptions(CsvReader csv)
    {
        csv.Context.TypeConverterOptionsCache.AddOptions<DateTime>(new TypeConverterOptions
        {
            CultureInfo = FrenchCulture,
            Formats = new[]
            {
                "dd/MM/yyyy",
                "d/MM/yyyy",
                "dd/MM/yyyy HH:mm:ss",
                "d/M/yyyy",
                "dd-MM-yyyy",
                "d-MM-yyyy",
                "dd/MM/yy",
                "d/M/yy"
            }
        });
    }

    private static bool TryParseDateFr(string input, out DateTime value)
    {
        return DateTime.TryParse(input, FrenchCulture, DateTimeStyles.None, out value);
    }

    // ── Records pour les nouvelles sources ──

    private class PotentielRecord
    {
        public string Matricule { get; set; } = string.Empty;
        public DateTime DateEvaluation { get; set; }
        public double AgiliteApprentissage { get; set; }
        public double Leadership { get; set; }
        public double Adaptabilite { get; set; }
        public double GestionComplexite { get; set; }
        public double Mobilite { get; set; }
        public double VisionStrategique { get; set; }
        public double Autonomie { get; set; }
    }

    private class FormationRecord
    {
        public string Matricule { get; set; } = string.Empty;
        public DateTime DateFormation { get; set; }
        public string Intitule { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public int DureeHeures { get; set; }
    }

    private class MobiliteRecord
    {
        public string Matricule { get; set; } = string.Empty;
        public DateTime DateDemande { get; set; }
        public string TypeMobilite { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public double ScoreMobilite { get; set; }
    }

    private class SuccessionRecord
    {
        public string Matricule { get; set; } = string.Empty;
        public string PosteCritiqueTitre { get; set; } = string.Empty;
        public string MatriculeSuccesseur { get; set; } = string.Empty;
        public string Readiness { get; set; } = string.Empty;
        public int Priorite { get; set; }
        public DateTime DateMiseAJour { get; set; }
    }

    private class CarriereRecord
    {
        public string Matricule { get; set; } = string.Empty;
        public DateTime DateMouvement { get; set; }
        public string TypeMouvement { get; set; } = string.Empty;
        public string PosteAvant { get; set; } = string.Empty;
        public string PosteApres { get; set; } = string.Empty;
        public string DirectionAvant { get; set; } = string.Empty;
        public string DirectionApres { get; set; } = string.Empty;
    }

    private class ReconnaissanceRecord
    {
        public string Matricule { get; set; } = string.Empty;
        public DateTime DateReconnaissance { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    private class LeadershipRecord
    {
        public string Matricule { get; set; } = string.Empty;
        public DateTime DateEvaluation { get; set; }
        public double ScoreLeadership { get; set; }
        public double ScoreEquipe { get; set; }
        public double ScorePairs { get; set; }
        public double ScoreHierarchie { get; set; }
    }

    private class VigilanceRecord
    {
        public string Matricule { get; set; } = string.Empty;
        public DateTime DateEvaluation { get; set; }
        public bool BaissePerformance { get; set; }
        public double? ScorePerformanceN1 { get; set; }
        public string? Commentaire { get; set; }
    }
}

