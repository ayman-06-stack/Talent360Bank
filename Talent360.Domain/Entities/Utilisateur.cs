namespace Talent360.Domain.Entities;

public enum RoleUtilisateur
{
    Collaborateur,
    Manager,
    DRH,
    Comite
}

public class Utilisateur
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string MotDePasseHache { get; set; } = string.Empty;
    public RoleUtilisateur Role { get; set; }
    public int TentativesEchouees { get; set; }
    public bool CompteVerrouille { get; set; }
}
