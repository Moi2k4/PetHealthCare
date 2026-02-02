using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PetCare.Infrastructure.Data;
using PetCare.Infrastructure.Repositories.Interfaces;

namespace PetCare.Infrastructure.Repositories.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly PetCareDbContext _context;
    private IDbContextTransaction? _transaction;
    private readonly Dictionary<Type, object> _repositories;

    // Specific repositories
    private IUserRepository? _userRepository;
    private IPetRepository? _petRepository;
    private IProductRepository? _productRepository;
    private IOrderRepository? _orderRepository;
    private IAppointmentRepository? _appointmentRepository;
    private IBlogPostRepository? _blogPostRepository;
    private IServiceRepository? _serviceRepository;

    public UnitOfWork(PetCareDbContext context)
    {
        _context = context;
        _repositories = new Dictionary<Type, object>();
    }

    public IUserRepository Users => _userRepository ??= new UserRepository(_context);
    public IPetRepository Pets => _petRepository ??= new PetRepository(_context);
    public IProductRepository Products => _productRepository ??= new ProductRepository(_context);
    public IOrderRepository Orders => _orderRepository ??= new OrderRepository(_context);
    public IAppointmentRepository Appointments => _appointmentRepository ??= new AppointmentRepository(_context);
    public IBlogPostRepository BlogPosts => _blogPostRepository ??= new BlogPostRepository(_context);
    public IServiceRepository Services => _serviceRepository ??= new ServiceRepository(_context);

    public IGenericRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        
        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new GenericRepository<T>(_context);
        }
        
        return (IGenericRepository<T>)_repositories[type];
    }

    public PetCareDbContext GetContext()
    {
        return _context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
