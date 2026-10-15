using Talent360.Application.Models;

namespace Talent360.Application.Interfaces;

public interface IComiteService
{
    List<ComiteMember> ObtenirMembresProposes(int top = 10);
}
