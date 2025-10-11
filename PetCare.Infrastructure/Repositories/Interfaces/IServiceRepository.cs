using PetCare.Domain.Entities;

namespace PetCare.Infrastructure.Repositories.Interfaces;

public interface IServiceRepository : IGenericRepository<Service>
{
    Task<IEnumerable<Service>> GetActiveServicesAsync();
    Task<IEnumerable<Service>> GetServicesByCategoryAsync(Guid categoryId);
    Task<Service?> GetServiceWithDetailsAsync(Guid serviceId);
}
