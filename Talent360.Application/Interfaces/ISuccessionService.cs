using Talent360.Domain.Entities;

namespace Talent360.Application.Interfaces;

public interface ISuccessionService
{
    /// <summary>
    /// Propose up to <paramref name="max"/> successeurs pour un poste critique donné.
    /// </summary>
    List<Collaborateur> ProposerSuccesseurs(int posteCritiqueId, int max = 3);
}
