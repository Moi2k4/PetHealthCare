using PetCare.Domain.Entities;

namespace PetCare.Infrastructure.Repositories.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetUserWithRoleAsync(Guid userId);
    Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);
    Task<bool> EmailExistsAsync(string email);
}
