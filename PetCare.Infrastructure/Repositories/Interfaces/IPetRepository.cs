using PetCare.Domain.Entities;

namespace PetCare.Infrastructure.Repositories.Interfaces;

public interface IPetRepository : IGenericRepository<Pet>
{
    Task<IEnumerable<Pet>> GetPetsByUserIdAsync(Guid userId);
    Task<Pet?> GetPetWithDetailsAsync(Guid petId);
    Task<IEnumerable<Pet>> GetActivePetsAsync(Guid userId);
}
