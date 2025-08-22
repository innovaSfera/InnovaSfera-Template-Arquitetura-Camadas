using InnovaSfera.Template.Domain.Entities;

namespace InnovaSfera.Template.Domain.Interfaces.External;

public interface IHarryPotterApiManager
{
    Task<ICollection<Character>> WizardsGetAllAsync();
}
