namespace Talent360.Infrastructure.Import;

/// <summary>
/// Données d'aperçu générées par un import AVANT écriture en base.
/// Permet à l'utilisateur de vérifier les données avant confirmation.
/// </summary>
public class ImportPreviewData
{
    /// <summary>Nom de la source (ex: "01 — SIRH", "04 — Potentiel").</summary>
    public string NomSource { get; set; } = string.Empty;

    /// <summary>Nombre total de lignes détectées dans le fichier.</summary>
    public int TotalLignes { get; set; }

    /// <summary>Nombre de matricules qui n'existent pas encore en base → seront insérés.</summary>
    public int NouveauxMatricules { get; set; }

    /// <summary>Nombre de matricules déjà existants → seront mis à jour (upsert).</summary>
    public int MatriculesMisAJour { get; set; }

    /// <summary>En-têtes des colonnes du fichier.</summary>
    public List<string> Colonnes { get; set; } = new();

    /// <summary>Les 5 premières lignes du fichier pour vérification visuelle.</summary>
    public List<string[]> AperçuLignes { get; set; } = new();

    /// <summary>Liste des matricules détectés dans le fichier.</summary>
    public List<string> MatriculesFichier { get; set; } = new();
}
