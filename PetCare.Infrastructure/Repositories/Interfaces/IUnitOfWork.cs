namespace PetCare.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Unit of Work interface for managing transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Repositories
    IUserRepository Users { get; }
    IPetRepository Pets { get; }
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    IAppointmentRepository Appointments { get; }
    IBlogPostRepository BlogPosts { get; }
    IServiceRepository Services { get; }
    
    // Generic repository access
    IGenericRepository<T> Repository<T>() where T : class;

    // Transaction methods
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
