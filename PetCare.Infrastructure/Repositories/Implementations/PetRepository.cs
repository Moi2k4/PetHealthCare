using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Data;
using PetCare.Infrastructure.Repositories.Interfaces;

namespace PetCare.Infrastructure.Repositories.Implementations;

public class PetRepository : GenericRepository<Pet>, IPetRepository
{
    public PetRepository(PetCareDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Pet>> GetPetsByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(p => p.Species)
            .Include(p => p.Breed)
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }

    public async Task<Pet?> GetPetWithDetailsAsync(Guid petId)
    {
        return await _dbSet
            .Include(p => p.Species)
            .Include(p => p.Breed)
            .Include(p => p.User)
            .Include(p => p.HealthRecords)
            .Include(p => p.Vaccinations)
            .FirstOrDefaultAsync(p => p.Id == petId);
    }

    public async Task<IEnumerable<Pet>> GetActivePetsAsync(Guid userId)
    {
        return await _dbSet
            .Include(p => p.Species)
            .Include(p => p.Breed)
            .Where(p => p.UserId == userId && p.IsActive)
            .ToListAsync();
    }
}
