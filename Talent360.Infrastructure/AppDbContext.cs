using Microsoft.EntityFrameworkCore;
using Talent360.Domain.Entities;

namespace Talent360.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<Collaborateur> Collaborateurs => Set<Collaborateur>();
    public DbSet<Poste> Postes => Set<Poste>();
    public DbSet<Competence> Competences => Set<Competence>();
    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();
    public DbSet<Parametre> Parametres => Set<Parametre>();
    public DbSet<Direction> Directions => Set<Direction>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<PosteCritique> PostesCritiques => Set<PosteCritique>();
    public DbSet<PlanDeveloppement> PlansDeveloppement => Set<PlanDeveloppement>();
    public DbSet<ActionDeveloppement> ActionsDeveloppement => Set<ActionDeveloppement>();
    public DbSet<HistoriqueEvenement> HistoriqueEvenements => Set<HistoriqueEvenement>();

    // Import CSV — sources existantes
    public DbSet<ImportAutoEvaluation> ImportAutoEvaluations => Set<ImportAutoEvaluation>();
    public DbSet<CompetenceNote> CompetenceNotes => Set<CompetenceNote>();
    public DbSet<ImportEvaluationManager> ImportEvaluationManagers => Set<ImportEvaluationManager>();
    public DbSet<ImportEngagement> ImportEngagements => Set<ImportEngagement>();

    // Import CSV — 8 nouvelles sources (Étape 3)
    public DbSet<ImportPotentiel> ImportPotentiels => Set<ImportPotentiel>();
    public DbSet<ImportFormation> ImportFormations => Set<ImportFormation>();
    public DbSet<ImportMobilite> ImportMobilites => Set<ImportMobilite>();
    public DbSet<ImportSuccession> ImportSuccessions => Set<ImportSuccession>();
    public DbSet<ImportCarriere> ImportCarrieres => Set<ImportCarriere>();
    public DbSet<ImportReconnaissance> ImportReconnaissances => Set<ImportReconnaissance>();
    public DbSet<ImportLeadership> ImportLeaderships => Set<ImportLeadership>();
    public DbSet<ImportVigilance> ImportVigilances => Set<ImportVigilance>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dossier = Path.Combine(appData, "Talent360Bank");
        Directory.CreateDirectory(dossier);
        var cheminDb = Path.Combine(dossier, "talent360.db");

        optionsBuilder.UseSqlite($"Data Source={cheminDb}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Collaborateur>(e =>
        {
            e.HasIndex(c => c.Matricule).IsUnique();
            e.HasOne(c => c.PlanDeveloppement)
             .WithOne(p => p.Collaborateur)
             .HasForeignKey<PlanDeveloppement>(p => p.CollaborateurId);
        });

        modelBuilder.Entity<Utilisateur>(e =>
        {
            e.HasIndex(u => u.Login).IsUnique();
        });

        modelBuilder.Entity<Parametre>(e =>
        {
            e.HasIndex(p => p.Cle).IsUnique();
        });

        modelBuilder.Entity<PlanDeveloppement>(e =>
        {
            e.HasMany(p => p.Actions)
             .WithOne(a => a.PlanDeveloppement)
             .HasForeignKey(a => a.PlanDeveloppementId);
        });

        // Import CSV : relation AutoEvaluation → CompetenceNote
        modelBuilder.Entity<ImportAutoEvaluation>(e =>
        {
            e.HasMany(a => a.Competences)
             .WithOne(c => c.ImportAutoEvaluation)
             .HasForeignKey(c => c.ImportAutoEvaluationId);
        });
    }
}


