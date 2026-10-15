namespace Talent360.Domain.Entities;

public class PosteCritique
{
    public int Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public string Criticite { get; set; } = string.Empty;
    public int MinSuccesseurs { get; set; }
}
