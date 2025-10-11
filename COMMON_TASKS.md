# 🛠️ Common Tasks & Code Snippets

## Table of Contents
1. [Adding New Entities](#adding-new-entities)
2. [Creating Services](#creating-services)
3. [Adding Controllers](#adding-controllers)
4. [Database Operations](#database-operations)
5. [AutoMapper Configurations](#automapper-configurations)
6. [Common Queries](#common-queries)

---

## 1. Adding New Entities

### Step 1: Create Entity Class

**Location**: `PetCare.Domain/Entities/YourEntity.cs`

```csharp
namespace PetCare.Domain.Entities;

using PetCare.Domain.Common;

public class YourEntity : AuditableEntity  // or BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Foreign Keys
    public Guid? CategoryId { get; set; }
    
    // Navigation Properties
    public virtual Category? Category { get; set; }
    public virtual ICollection<RelatedEntity> RelatedEntities { get; set; } = new List<RelatedEntity>();
}
```

### Step 2: Add to DbContext

**Location**: `PetCare.Infrastructure/Data/PetCareDbContext.cs`

```csharp
// Add DbSet
public DbSet<YourEntity> YourEntities { get; set; }

// Configure in OnModelCreating
private void ConfigureYourEntities(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<YourEntity>(entity =>
    {
        entity.ToTable("your_entities");  // snake_case
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
        entity.Property(e => e.Description).HasColumnName("description");
        entity.Property(e => e.IsActive).HasColumnName("is_active");
        entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        // Configure relationships
        entity.HasOne(e => e.Category)
            .WithMany(c => c.YourEntities)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Add indexes
        entity.HasIndex(e => e.Name);
    });
}

// Call in OnModelCreating
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    // ... other configurations
    ConfigureYourEntities(modelBuilder);
}
```

### Step 3: Create Migration

```bash
dotnet ef migrations add AddYourEntity --project PetCare.Infrastructure --startup-project PetCare.API
dotnet ef database update --project PetCare.Infrastructure --startup-project PetCare.API
```

---

## 2. Creating Services

### Step 1: Create DTOs

**Location**: `PetCare.Application/DTOs/YourEntity/`

```csharp
// YourEntityDto.cs
public class YourEntityDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// CreateYourEntityDto.cs
public class CreateYourEntityDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
}

// UpdateYourEntityDto.cs
public class UpdateYourEntityDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
```

### Step 2: Create Repository Interface

**Location**: `PetCare.Infrastructure/Repositories/Interfaces/IYourEntityRepository.cs`

```csharp
using PetCare.Domain.Entities;

namespace PetCare.Infrastructure.Repositories.Interfaces;

public interface IYourEntityRepository : IGenericRepository<YourEntity>
{
    Task<IEnumerable<YourEntity>> GetActiveEntitiesAsync();
    Task<YourEntity?> GetByNameAsync(string name);
    Task<IEnumerable<YourEntity>> GetByCategoryAsync(Guid categoryId);
}
```

### Step 3: Implement Repository

**Location**: `PetCare.Infrastructure/Repositories/Implementations/YourEntityRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Data;
using PetCare.Infrastructure.Repositories.Interfaces;

namespace PetCare.Infrastructure.Repositories.Implementations;

public class YourEntityRepository : GenericRepository<YourEntity>, IYourEntityRepository
{
    public YourEntityRepository(PetCareDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<YourEntity>> GetActiveEntitiesAsync()
    {
        return await _dbSet
            .Include(e => e.Category)
            .Where(e => e.IsActive)
            .ToListAsync();
    }

    public async Task<YourEntity?> GetByNameAsync(string name)
    {
        return await _dbSet
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Name == name);
    }

    public async Task<IEnumerable<YourEntity>> GetByCategoryAsync(Guid categoryId)
    {
        return await _dbSet
            .Where(e => e.CategoryId == categoryId)
            .ToListAsync();
    }
}
```

### Step 4: Add to Unit of Work

**Location**: `PetCare.Infrastructure/Repositories/Interfaces/IUnitOfWork.cs`

```csharp
public interface IUnitOfWork : IDisposable
{
    // ... existing repositories
    IYourEntityRepository YourEntities { get; }
}
```

**Location**: `PetCare.Infrastructure/Repositories/Implementations/UnitOfWork.cs`

```csharp
private IYourEntityRepository? _yourEntityRepository;

public IYourEntityRepository YourEntities => 
    _yourEntityRepository ??= new YourEntityRepository(_context);
```

### Step 5: Create Service Interface

**Location**: `PetCare.Application/Services/Interfaces/IYourEntityService.cs`

```csharp
using PetCare.Application.DTOs.YourEntity;
using PetCare.Application.Common;

namespace PetCare.Application.Services.Interfaces;

public interface IYourEntityService
{
    Task<ServiceResult<YourEntityDto>> GetByIdAsync(Guid id);
    Task<ServiceResult<PagedResult<YourEntityDto>>> GetAllAsync(int page, int pageSize);
    Task<ServiceResult<YourEntityDto>> CreateAsync(CreateYourEntityDto dto);
    Task<ServiceResult<YourEntityDto>> UpdateAsync(Guid id, UpdateYourEntityDto dto);
    Task<ServiceResult<bool>> DeleteAsync(Guid id);
    Task<ServiceResult<IEnumerable<YourEntityDto>>> GetActiveAsync();
}
```

### Step 6: Implement Service

**Location**: `PetCare.Application/Services/Implementations/YourEntityService.cs`

```csharp
using AutoMapper;
using PetCare.Application.DTOs.YourEntity;
using PetCare.Application.Common;
using PetCare.Application.Services.Interfaces;
using PetCare.Infrastructure.Repositories.Interfaces;
using PetCare.Domain.Entities;

namespace PetCare.Application.Services.Implementations;

public class YourEntityService : IYourEntityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public YourEntityService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<YourEntityDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _unitOfWork.YourEntities.GetByIdAsync(id);
            
            if (entity == null)
            {
                return ServiceResult<YourEntityDto>.FailureResult("Entity not found");
            }

            var dto = _mapper.Map<YourEntityDto>(entity);
            return ServiceResult<YourEntityDto>.SuccessResult(dto);
        }
        catch (Exception ex)
        {
            return ServiceResult<YourEntityDto>.FailureResult($"Error: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PagedResult<YourEntityDto>>> GetAllAsync(int page, int pageSize)
    {
        try
        {
            var (entities, totalCount) = await _unitOfWork.YourEntities.GetPagedAsync(
                page,
                pageSize,
                orderBy: q => q.OrderBy(e => e.Name),
                includes: e => e.Category!
            );

            var dtos = _mapper.Map<IEnumerable<YourEntityDto>>(entities);
            
            var pagedResult = new PagedResult<YourEntityDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<YourEntityDto>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            return ServiceResult<PagedResult<YourEntityDto>>.FailureResult($"Error: {ex.Message}");
        }
    }

    public async Task<ServiceResult<YourEntityDto>> CreateAsync(CreateYourEntityDto dto)
    {
        try
        {
            var entity = _mapper.Map<YourEntity>(dto);
            await _unitOfWork.YourEntities.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var resultDto = _mapper.Map<YourEntityDto>(entity);
            return ServiceResult<YourEntityDto>.SuccessResult(resultDto, "Created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<YourEntityDto>.FailureResult($"Error: {ex.Message}");
        }
    }

    public async Task<ServiceResult<YourEntityDto>> UpdateAsync(Guid id, UpdateYourEntityDto dto)
    {
        try
        {
            var entity = await _unitOfWork.YourEntities.GetByIdAsync(id);
            
            if (entity == null)
            {
                return ServiceResult<YourEntityDto>.FailureResult("Entity not found");
            }

            _mapper.Map(dto, entity);
            await _unitOfWork.YourEntities.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var resultDto = _mapper.Map<YourEntityDto>(entity);
            return ServiceResult<YourEntityDto>.SuccessResult(resultDto, "Updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<YourEntityDto>.FailureResult($"Error: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _unitOfWork.YourEntities.GetByIdAsync(id);
            
            if (entity == null)
            {
                return ServiceResult<bool>.FailureResult("Entity not found");
            }

            await _unitOfWork.YourEntities.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true, "Deleted successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<YourEntityDto>>> GetActiveAsync()
    {
        try
        {
            var entities = await _unitOfWork.YourEntities.GetActiveEntitiesAsync();
            var dtos = _mapper.Map<IEnumerable<YourEntityDto>>(entities);
            
            return ServiceResult<IEnumerable<YourEntityDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<YourEntityDto>>.FailureResult($"Error: {ex.Message}");
        }
    }
}
```

### Step 7: Add AutoMapper Mapping

**Location**: `PetCare.Application/Mappings/MappingProfile.cs`

```csharp
public MappingProfile()
{
    // ... existing mappings
    
    // YourEntity mappings
    CreateMap<YourEntity, YourEntityDto>();
    CreateMap<CreateYourEntityDto, YourEntity>();
    CreateMap<UpdateYourEntityDto, YourEntity>()
        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
}
```

### Step 8: Register Services in Program.cs

**Location**: `PetCare.API/Program.cs`

```csharp
// Register repositories
builder.Services.AddScoped<IYourEntityRepository, YourEntityRepository>();

// Register services
builder.Services.AddScoped<IYourEntityService, YourEntityService>();
```

---

## 3. Adding Controllers

**Location**: `PetCare.API/Controllers/YourEntitiesController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.Services.Interfaces;
using PetCare.Application.DTOs.YourEntity;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class YourEntitiesController : ControllerBase
{
    private readonly IYourEntityService _service;

    public YourEntitiesController(IYourEntityService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get entity by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Get all entities with pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get all active entities
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var result = await _service.GetActiveAsync();
        return Ok(result);
    }

    /// <summary>
    /// Create new entity
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateYourEntityDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _service.CreateAsync(dto);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update entity
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateYourEntityDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _service.UpdateAsync(id, dto);
        
        if (!result.Success)
        {
            return NotFound(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Delete entity
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        
        if (!result.Success)
        {
            return NotFound(result);
        }
        
        return Ok(result);
    }
}
```

---

## 4. Database Operations

### Complex Query with Multiple Includes

```csharp
public async Task<Order?> GetOrderWithFullDetailsAsync(Guid orderId)
{
    return await _dbSet
        .Include(o => o.User)
            .ThenInclude(u => u.Role)
        .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Category)
        .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Images)
        .Include(o => o.StatusHistory)
        .FirstOrDefaultAsync(o => o.Id == orderId);
}
```

### Pagination with Filter and Sort

```csharp
public async Task<(IEnumerable<Product> Items, int TotalCount)> GetProductsPagedAsync(
    int page,
    int pageSize,
    string? searchTerm,
    Guid? categoryId,
    decimal? minPrice,
    decimal? maxPrice)
{
    var query = _dbSet
        .Include(p => p.Category)
        .Include(p => p.Brand)
        .Include(p => p.Images)
        .Where(p => p.IsActive);

    // Apply filters
    if (!string.IsNullOrEmpty(searchTerm))
    {
        query = query.Where(p => p.ProductName.Contains(searchTerm) || 
                                 p.Description!.Contains(searchTerm));
    }

    if (categoryId.HasValue)
    {
        query = query.Where(p => p.CategoryId == categoryId);
    }

    if (minPrice.HasValue)
    {
        query = query.Where(p => p.Price >= minPrice);
    }

    if (maxPrice.HasValue)
    {
        query = query.Where(p => p.Price <= maxPrice);
    }

    var totalCount = await query.CountAsync();

    var items = await query
        .OrderBy(p => p.ProductName)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return (items, totalCount);
}
```

### Transaction Example

```csharp
public async Task<ServiceResult<OrderDto>> CreateOrderAsync(CreateOrderDto dto)
{
    try
    {
        await _unitOfWork.BeginTransactionAsync();

        // Create order
        var order = _mapper.Map<Order>(dto);
        await _unitOfWork.Orders.AddAsync(order);

        // Update stock
        foreach (var item in dto.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                product.StockQuantity -= item.Quantity;
                await _unitOfWork.Products.UpdateAsync(product);
            }
        }

        // Clear cart
        var cartItems = await _unitOfWork.Repository<CartItem>()
            .FindAsync(c => c.UserId == dto.UserId);
        await _unitOfWork.Repository<CartItem>().DeleteRangeAsync(cartItems);

        await _unitOfWork.CommitTransactionAsync();

        var orderDto = _mapper.Map<OrderDto>(order);
        return ServiceResult<OrderDto>.SuccessResult(orderDto, "Order created successfully");
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackTransactionAsync();
        return ServiceResult<OrderDto>.FailureResult($"Error creating order: {ex.Message}");
    }
}
```

---

## 5. AutoMapper Configurations

### Complex Mapping

```csharp
// Mapping with nested objects
CreateMap<Order, OrderDto>()
    .ForMember(dest => dest.UserName, 
               opt => opt.MapFrom(src => src.User.FullName))
    .ForMember(dest => dest.OrderItems, 
               opt => opt.MapFrom(src => src.OrderItems));

// Conditional mapping
CreateMap<UpdateUserDto, User>()
    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

// Custom value resolver
CreateMap<Product, ProductDto>()
    .ForMember(dest => dest.DisplayPrice, 
               opt => opt.MapFrom(src => src.SalePrice ?? src.Price))
    .ForMember(dest => dest.IsOnSale, 
               opt => opt.MapFrom(src => src.SalePrice.HasValue));
```

---

## 6. Common Queries

### Aggregate Functions

```csharp
// Count
var totalProducts = await _dbSet.CountAsync(p => p.IsActive);

// Sum
var totalRevenue = await _context.Orders
    .Where(o => o.OrderStatus == "completed")
    .SumAsync(o => o.FinalAmount);

// Average
var averagePrice = await _dbSet
    .Where(p => p.IsActive)
    .AverageAsync(p => p.Price);

// Group by
var productsByCategory = await _context.Products
    .GroupBy(p => p.Category!.CategoryName)
    .Select(g => new
    {
        Category = g.Key,
        Count = g.Count(),
        AveragePrice = g.Average(p => p.Price)
    })
    .ToListAsync();
```

### Date Queries

```csharp
// Today's appointments
var today = DateTime.Today;
var todayAppointments = await _dbSet
    .Where(a => a.AppointmentDate.Date == today)
    .ToListAsync();

// This month's orders
var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
var monthlyOrders = await _context.Orders
    .Where(o => o.OrderedAt >= firstDayOfMonth)
    .ToListAsync();

// Date range
var startDate = new DateTime(2024, 1, 1);
var endDate = new DateTime(2024, 12, 31);
var ordersInRange = await _context.Orders
    .Where(o => o.OrderedAt >= startDate && o.OrderedAt <= endDate)
    .ToListAsync();
```

---

**For more examples and best practices, refer to the existing code in the solution!**
