using Talent360.Domain.Entities;

namespace Talent360.Infrastructure.Seed;

/// <summary>
/// Générateur de données fictives réalistes pour l'environnement de démonstration.
/// Traduit de la logique du prototype JavaScript Talent 360 Bank.
/// </summary>
public static class DonneesFictivesSeeder
{
    // ──────────────────────────────────────────────
    //  PRNG déterministe (Mulberry32 porté en C#)
    // ──────────────────────────────────────────────
    private static uint _seed = 20260909;

    private static double Rnd()
    {
        unchecked
        {
            _seed |= 0;
            _seed += 0x6D2B79F5;
            uint t = (uint)((long)(_seed ^ (_seed >> 15)) * (1 | _seed));
            t = (uint)((long)(t + (uint)((long)(t ^ (t >> 7)) * (61 | t))) ^ t);
            return ((t ^ (t >> 14)) >> 0) / 4294967296.0;
        }
    }

    private static int RandInt(int min, int max) => (int)Math.Floor(Rnd() * (max - min + 1)) + min;
    private static double RandFloat(double min, double max) => Rnd() * (max - min) + min;
    private static T Choice<T>(T[] arr) => arr[RandInt(0, arr.Length - 1)];

    private static T[] Shuffle<T>(T[] arr)
    {
        var a = (T[])arr.Clone();
        for (int i = a.Length - 1; i > 0; i--)
        {
            int j = RandInt(0, i);
            (a[i], a[j]) = (a[j], a[i]);
        }
        return a;
    }

    // ──────────────────────────────────────────────
    //  RÉFÉRENTIELS (fidèles au prototype JS)
    // ──────────────────────────────────────────────
    private static readonly string[] PrenomsM =
    {
        "Youssef","Karim","Anas","Reda","Mehdi","Yassine","Nabil","Hamza","Adil","Omar",
        "Amine","Rachid","Khalid","Tarik","Younes","Ismail","Zakaria","Bilal","Fouad","Samir",
        "Hicham","Mounir","Othmane","Walid","Driss"
    };

    private static readonly string[] PrenomsF =
    {
        "Sara","Ghita","Hanane","Nadia","Salma","Imane","Meryem","Fatine","Loubna","Asmaa",
        "Kenza","Rania","Zineb","Siham","Nawal","Ilham","Houda","Dounia","Amal","Latifa"
    };

    private static readonly string[] Noms =
    {
        "El Amrani","Bennani","Tazi","Chraibi","Alaoui","Benjelloun","Fassi","Idrissi",
        "Cherkaoui","Berrada","Squalli","Lahlou","Sbai","Guessous","Belhaj","Andaloussi",
        "Zouine","Kabbaj","Oujdi","Bouzidi","Bakkali","Rifai","Sekkat","Amrani","Haddadi",
        "Chafai","Naciri","Tahiri","Filali","Mekouar"
    };

    private record DirectionDef(string Nom, int Count, string[] Postes);

    private static readonly DirectionDef[] DirectionsDef =
    {
        new("Réseau commercial", 35, new[]{"Chargé de clientèle","Conseiller clientèle senior","Directeur d'agence","Responsable commercial régional","Directeur régional"}),
        new("Risques & conformité", 15, new[]{"Analyste risques","Chargé de conformité","Risk Manager","Responsable conformité","Directeur des risques"}),
        new("Finance", 10, new[]{"Analyste financier","Contrôleur de gestion","Responsable comptabilité","Trésorier","Directeur financier"}),
        new("RH", 8, new[]{"Chargé RH","Business partner RH","Responsable formation","Responsable recrutement","Directeur RH"}),
        new("IT & Digital", 12, new[]{"Développeur","Data analyst","Chef de projet digital","Architecte SI","Responsable cybersécurité","Directeur IT & Digital"}),
        new("Opérations", 10, new[]{"Gestionnaire back-office","Analyste process","Responsable back-office","Directeur des opérations"}),
        new("Audit", 5, new[]{"Auditeur interne","Auditeur senior","Responsable audit","Directeur audit"}),
        new("Juridique", 5, new[]{"Juriste","Juriste senior","Responsable juridique","Directeur juridique"})
    };

    private static readonly string[] GradesDef =
        { "G6","G7","G8","G9","G10","G11","G12","G13","G14","G15" };

    private static readonly (string Nom, string Categorie)[] CompetencesDef =
    {
        // Techniques (12)
        ("Expertise bancaire", "Technique"),
        ("Analyse financière", "Technique"),
        ("Gestion des risques", "Technique"),
        ("Conformité", "Technique"),
        ("Crédit", "Technique"),
        ("Relation client", "Technique"),
        ("Data Analytics", "Technique"),
        ("Digital Banking", "Technique"),
        ("Cybersécurité", "Technique"),
        ("Automatisation", "Technique"),
        ("Comptabilité IFRS", "Technique"),
        ("Audit interne", "Technique"),
        // Managériales (6)
        ("Leadership", "Managériale"),
        ("Management", "Managériale"),
        ("Prise de décision", "Managériale"),
        ("Vision stratégique", "Managériale"),
        ("Conduite du changement", "Managériale"),
        ("Gestion de projet", "Managériale"),
        // Comportementales (7)
        ("Communication", "Comportementale"),
        ("Collaboration", "Comportementale"),
        ("Adaptabilité", "Comportementale"),
        ("Learning agility", "Comportementale"),
        ("Négociation", "Comportementale"),
        ("Orientation résultats", "Comportementale"),
        ("Intelligence émotionnelle", "Comportementale")
    };

    private static readonly (string Titre, string Direction, string Criticite, int MinSuccesseurs)[] PostesCritiquesDef =
    {
        ("Directeur régional", "Réseau commercial", "Stratégique", 2),
        ("Responsable Risques", "Risques & conformité", "Stratégique", 2),
        ("Directeur Financier", "Finance", "Stratégique", 1),
        ("Directeur RH", "RH", "Stratégique", 1),
        ("Directeur IT & Digital", "IT & Digital", "Stratégique", 2),
        ("Directeur des Opérations", "Opérations", "Stratégique", 1),
        ("Responsable Conformité", "Risques & conformité", "Sensible", 1),
        ("Responsable Audit", "Audit", "Sensible", 1),
        ("Responsable Juridique", "Juridique", "Sensible", 1),
        ("Responsable Cybersécurité", "IT & Digital", "Sensible", 1),
        ("Responsable Commercial Régional", "Réseau commercial", "Sensible", 1),
        ("Chef de Projet Digital Senior", "IT & Digital", "Sensible", 1),
        ("Responsable Contrôle de Gestion", "Finance", "Sensible", 1),
        ("Responsable Formation", "RH", "Sensible", 1),
        ("Responsable Back-Office", "Opérations", "Sensible", 1)
    };

    // Catégories de profils pour distribution réaliste des scores (fidèle au JS)
    private record CategorieProfil(string Label, int N, int PerfMin, int PerfMax, int PotMin, int PotMax);

    private static readonly CategorieProfil[] CategoriesProfils =
    {
        new("Talent clé",          15, 85, 97, 85, 97),
        new("Haut potentiel",      10, 68, 84, 82, 96),
        new("Expert",              15, 85, 96, 52, 72),
        new("Manager performant",  15, 82, 94, 70, 84),
        new("Confirmé",            25, 64, 81, 58, 78),
        new("À développer",        10, 54, 68, 48, 66),
        new("À surveiller",        10, 34, 58, 34, 58)
    };

    // ──────────────────────────────────────────────
    //  POINT D'ENTRÉE
    // ──────────────────────────────────────────────
    public static void Executer(AppDbContext context)
    {
        // Réinitialiser le PRNG pour des résultats reproductibles
        _seed = 20260909;

        // Guard : ne pas ré-insérer si des données existent déjà
        if (context.Collaborateurs.Any())
            return;

        InsererReferentiels(context);
        GenererCollaborateurs(context);
        CreerUtilisateursTest(context);

        context.SaveChanges();
    }

    // ──────────────────────────────────────────────
    //  INSERTION DES RÉFÉRENTIELS
    // ──────────────────────────────────────────────
    private static void InsererReferentiels(AppDbContext context)
    {
        // Directions
        foreach (var d in DirectionsDef)
        {
            context.Directions.Add(new Direction { Nom = d.Nom });
        }

        // Grades (G6=6, G7=7, ..., G15=15)
        for (int i = 0; i < GradesDef.Length; i++)
        {
            context.Grades.Add(new Grade { Nom = GradesDef[i], Niveau = i + 6 });
        }

        // 25 compétences
        foreach (var (nom, cat) in CompetencesDef)
        {
            context.Competences.Add(new Competence { Nom = nom, Categorie = cat });
        }

        // Postes critiques
        foreach (var (titre, dir, crit, min) in PostesCritiquesDef)
        {
            context.PostesCritiques.Add(new PosteCritique
            {
                Titre = titre,
                Direction = dir,
                Criticite = crit,
                MinSuccesseurs = min
            });
        }

        // Postes (un par titre unique dans les directions)
        var postesDejaAjoutes = new HashSet<string>();
        foreach (var dir in DirectionsDef)
        {
            foreach (var poste in dir.Postes)
            {
                if (postesDejaAjoutes.Add(poste))
                {
                    context.Postes.Add(new Poste
                    {
                        Titre = poste,
                        Direction = dir.Nom,
                        Criticite = PostesCritiquesDef.Any(pc => pc.Titre == poste) ? "Critique" : "Standard"
                    });
                }
            }
        }

        // Paramètres de calcul (seuils et pondérations) — spécification banque v2
        var parametres = new (string Cle, string Valeur)[]
        {
            // ── Pondérations Performance (5 axes) ──
            ("PoidsPerfObjectifs", "0.40"),
            ("PoidsPerfCompetences", "0.20"),
            ("PoidsPerfComportement", "0.20"),
            ("PoidsPerfContribution", "0.10"),
            ("PoidsPerfDeveloppement", "0.10"),

            // ── Pondérations Potentiel (7 axes) ──
            ("PoidsPotAgiliteApprentissage", "0.20"),
            ("PoidsPotLeadership", "0.20"),
            ("PoidsPotAdaptabilite", "0.15"),
            ("PoidsPotGestionComplexite", "0.15"),
            ("PoidsPotMobilite", "0.10"),
            ("PoidsPotVisionStrategique", "0.10"),
            ("PoidsPotAutonomie", "0.10"),

            // ── Seuils 9-Box (séparés Perf/Pot) ──
            ("Seuil9BoxPerfEleve", "85"),
            ("Seuil9BoxPerfMoyen", "70"),
            ("Seuil9BoxPotEleve", "85"),
            ("Seuil9BoxPotMoyen", "70"),

            // ── Catégories de Performance (étiquettes individuelles) ──
            ("SeuilPerfExceptionnelle", "90"),
            ("SeuilPerfElevee", "80"),
            ("SeuilPerfSolide", "70"),
            ("SeuilPerfARenforcer", "60"),

            // ── Seuils Talent & Haut Potentiel (proposé automatiquement) ──
            ("SeuilTalentProposePerf", "85"),
            ("SeuilTalentProposePot", "85"),
            ("SeuilHautPotentielPot", "85"),
            ("SeuilHautPotentielPerf", "75"),

            // ── Pondérations Matching (adéquation poste) ──
            ("PoidsMatchingCompetences", "0.25"),
            ("PoidsMatchingPerformance", "0.20"),
            ("PoidsMatchingPotentiel", "0.20"),
            ("PoidsMatchingExperience", "0.15"),
            ("PoidsMatchingLeadership", "0.10"),
            ("PoidsMatchingMobilite", "0.10"),
            ("PointsExperienceParAn", "8"),
            ("PlafondExperience", "100"),

            // ── Seuils Readiness ──
            ("SeuilReadinessNow", "90"),
            ("SeuilReadiness1An", "80"),
            ("SeuilReadiness1a2Ans", "65"),

            // ── Vigilance RH (système de points cumulatifs) ──
            ("PointsVigilanceEngagement", "25"),
            ("SeuilVigilanceEngagement", "60"),
            ("PointsVigilanceAucunMouvement", "20"),
            ("SeuilVigilanceAnneesSansMouvement", "4"),
            ("PointsVigilanceMobiliteNonTraitee", "15"),
            ("PointsVigilanceAucunDev", "15"),
            ("PointsVigilanceBaissePerf", "10"),
            ("PointsVigilanceFaibleReconnaissance", "10"),
            ("PointsVigilanceFormationNonRealisee", "5"),
            ("SeuilVigilanceFaible", "30"),
            ("SeuilVigilanceModere", "60")
        };

        foreach (var (cle, valeur) in parametres)
        {
            context.Parametres.Add(new Parametre { Cle = cle, Valeur = valeur });
        }

        context.SaveChanges();
    }

    // ──────────────────────────────────────────────
    //  GÉNÉRATION DE 100 COLLABORATEURS
    // ──────────────────────────────────────────────
    private static readonly Dictionary<string, string> VivierMap = new()
    {
        ["Réseau commercial"] = "Commercial",
        ["Risques & conformité"] = "Risques",
        ["IT & Digital"] = "Digital",
        ["Finance"] = "Expertise",
        ["Audit"] = "Expertise",
        ["Juridique"] = "Expertise",
        ["RH"] = "Management",
        ["Opérations"] = "Management"
    };

    private static void GenererCollaborateurs(AppDbContext context)
    {
        // Construire le pool de catégories (identique au JS)
        var categoryPool = new List<CategorieProfil>();
        foreach (var cat in CategoriesProfils)
        {
            for (int i = 0; i < cat.N; i++)
                categoryPool.Add(cat);
        }
        var poolShuffled = Shuffle(categoryPool.ToArray());

        int idCounter = 0;
        foreach (var dir in DirectionsDef)
        {
            for (int i = 0; i < dir.Count; i++)
            {
                bool isMasculin = Rnd() < 0.58;
                string prenom = isMasculin ? Choice(PrenomsM) : Choice(PrenomsF);
                string nom = Choice(Noms);

                var cat = poolShuffled[idCounter];
                int perf = (int)Math.Round(RandFloat(cat.PerfMin, cat.PerfMax));
                int pot = (int)Math.Round(RandFloat(cat.PotMin, cat.PotMax));

                // Poste indexé sur la performance (comme dans le JS)
                int posteIdx = Math.Max(0, Math.Min(dir.Postes.Length - 1,
                    (int)Math.Round((perf / 100.0) * (dir.Postes.Length - 1) + RandInt(-1, 1))));

                int anneeEntree = RandInt(2008, 2023);
                var dateEntree = new DateTime(anneeEntree, RandInt(1, 12), RandInt(1, 28));

                // Grade basé sur l'index du poste (comme dans le JS)
                int gradeIdx = Math.Min(GradesDef.Length - 1, posteIdx * 2 + RandInt(0, 1));

                string matricule = $"BP{(idCounter + 1):D3}";

                // Talent proposé (même règle que le JS)
                bool talentPropose = perf >= 80 && pot >= 80;
                bool talentValide = talentPropose && Rnd() < 0.85;
                int engagement = RandInt(58, 96);

                // Vivier et matching (comme dans le JS)
                string? vivier = null;
                string? posteCible = null;
                double? matching = null;
                string? readiness = null;

                if (talentValide)
                {
                    vivier = (dir.Nom == "Réseau commercial" && Rnd() < 0.4)
                        ? "Direction"
                        : VivierMap.GetValueOrDefault(dir.Nom, "Expertise");
                }

                if (vivier != null)
                {
                    int nextIdx = Math.Min(dir.Postes.Length - 1, posteIdx + 1);
                    posteCible = dir.Postes[nextIdx] == dir.Postes[posteIdx]
                        ? "Poste de direction équivalent"
                        : dir.Postes[nextIdx];

                    double brut = perf * 0.40 + pot * 0.35 + engagement * 0.15 + RandFloat(0, 15) * 0.10;
                    matching = Math.Max(58, Math.Min(99, Math.Round(brut)));
                    readiness = matching >= 90 ? "Ready Now" : (matching >= 75 ? "<2 ans" : ">2 ans");
                }

                context.Collaborateurs.Add(new Collaborateur
                {
                    Matricule = matricule,
                    Nom = nom,
                    Prenom = prenom,
                    Direction = dir.Nom,
                    Poste = dir.Postes[posteIdx],
                    Grade = GradesDef[gradeIdx],
                    ManagerId = null,
                    SourceDonnees = SourceDonnees.Demonstration,
                    DateEntree = dateEntree,
                    ScorePerformance = perf,
                    ScorePotentiel = pot,
                    TalentPropose = talentPropose,
                    TalentValide = talentValide,
                    Engagement = engagement,
                    Vivier = vivier,
                    PosteCible = posteCible,
                    Matching = matching,
                    Readiness = readiness,
                    EstSoumise = false
                });

                idCounter++;
            }
        }
    }

    // ──────────────────────────────────────────────
    //  UTILISATEURS DE TEST (un par rôle)
    // ──────────────────────────────────────────────
    private static void CreerUtilisateursTest(AppDbContext context)
    {
        string hash = BCrypt.Net.BCrypt.HashPassword("Test1234");

        context.Utilisateurs.Add(new Utilisateur
        {
            Login = "collab",
            MotDePasseHache = hash,
            Role = RoleUtilisateur.Collaborateur,
            TentativesEchouees = 0,
            CompteVerrouille = false
        });

        context.Utilisateurs.Add(new Utilisateur
        {
            Login = "manager",
            MotDePasseHache = hash,
            Role = RoleUtilisateur.Manager,
            TentativesEchouees = 0,
            CompteVerrouille = false
        });

        context.Utilisateurs.Add(new Utilisateur
        {
            Login = "drh",
            MotDePasseHache = hash,
            Role = RoleUtilisateur.DRH,
            TentativesEchouees = 0,
            CompteVerrouille = false
        });

        context.Utilisateurs.Add(new Utilisateur
        {
            Login = "comite",
            MotDePasseHache = hash,
            Role = RoleUtilisateur.Comite,
            TentativesEchouees = 0,
            CompteVerrouille = false
        });
    }
}
