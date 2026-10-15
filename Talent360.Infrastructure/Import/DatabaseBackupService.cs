namespace Talent360.Infrastructure.Import;

/// <summary>
/// Service de sauvegarde automatique de la base SQLite avant chaque import confirmé.
/// Les backups sont stockés dans %AppData%\Talent360Bank\Backups\.
/// </summary>
public static class DatabaseBackupService
{
    /// <summary>
    /// Crée une copie horodatée de la base de données.
    /// Retourne le chemin complet du fichier de sauvegarde.
    /// </summary>
    public static string CreerSauvegarde()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dbPath = Path.Combine(appData, "Talent360Bank", "talent360.db");
        var backupDir = Path.Combine(appData, "Talent360Bank", "Backups");

        Directory.CreateDirectory(backupDir);

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupPath = Path.Combine(backupDir, $"talent360_{timestamp}.db");

        if (File.Exists(dbPath))
        {
            File.Copy(dbPath, backupPath, overwrite: true);
        }

        return backupPath;
    }

    /// <summary>
    /// Liste toutes les sauvegardes existantes, triées par date décroissante.
    /// </summary>
    public static List<(string Chemin, DateTime Date, long TailleMo)> ListerSauvegardes()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var backupDir = Path.Combine(appData, "Talent360Bank", "Backups");

        if (!Directory.Exists(backupDir))
            return new();

        return Directory.GetFiles(backupDir, "talent360_*.db")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.LastWriteTime)
            .Select(f => (f.FullName, f.LastWriteTime, f.Length / (1024 * 1024)))
            .ToList();
    }
}
