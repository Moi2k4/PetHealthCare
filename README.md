# PetCare Platform - Clean Architecture with Code-First Approach

## 🏗️ Architecture Overview

This project implements a **Clean Architecture** pattern with **Code-First approach** using:
- **Domain Layer**: Entity models
- **Infrastructure Layer**: DbContext, Repositories, Data Access
- **Application Layer**: Services, DTOs, AutoMapper profiles
- **API Layer**: Controllers, API endpoints

## 📁 Project Structure

```
PetCare/
├── PetCare.Domain/              # Domain Layer
│   ├── Common/
│   │   ├── BaseEntity.cs        # Base entity with Id, CreatedAt
│   │   └── AuditableEntity.cs   # Base entity with UpdatedAt
│   └── Entities/                # Domain entities
│       ├── User.cs
│       ├── Pet.cs
│       ├── Product.cs
│       ├── Order.cs
│       ├── Appointment.cs
│       └── ...
│
├── PetCare.Infrastructure/      # Infrastructure Layer
│   ├── Data/
│   │   └── PetCareDbContext.cs  # EF Core DbContext
│   └── Repositories/
│       ├── Interfaces/          # Repository interfaces
│       │   ├── IGenericRepository.cs
│       │   ├── IUnitOfWork.cs
│       │   ├── IUserRepository.cs
│       │   └── ...
│       └── Implementations/     # Repository implementations
│           ├── GenericRepository.cs
│           ├── UnitOfWork.cs
│           ├── UserRepository.cs
│           └── ...
│
├── PetCare.Application/         # Application Layer
│   ├── DTOs/                    # Data Transfer Objects
│   │   ├── User/
│   │   ├── Pet/
│   │   ├── Product/
│   │   └── ...
│   ├── Mappings/
│   │   └── MappingProfile.cs    # AutoMapper profiles
│   ├── Services/
│   │   ├── Interfaces/          # Service interfaces
│   │   │   ├── IUserService.cs
│   │   │   ├── IPetService.cs
│   │   │   └── ...
│   │   └── Implementations/     # Service implementations
│   │       ├── UserService.cs
│   │       ├── PetService.cs
│   │       └── ...
│   └── Common/
│       ├── ServiceResult.cs     # Service response wrapper
│       └── PagedResult.cs       # Pagination wrapper
│
└── PetCare.API/                 # API Layer
    ├── Controllers/
    │   ├── UsersController.cs
    │   ├── PetsController.cs
    │   └── ProductsController.cs
    ├── Program.cs               # Application configuration
    └── appsettings.json         # Configuration settings
```

## 🎯 Design Patterns Implemented

### 1. **Repository Pattern**
- Generic repository for common CRUD operations
- Specific repositories for entity-specific queries
- Abstraction over data access layer

```csharp
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    // ... more methods
}
```

### 2. **Unit of Work Pattern**
- Manages transactions across multiple repositories
- Ensures data consistency
- Single point to save all changes

```csharp
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IPetRepository Pets { get; }
    IProductRepository Products { get; }
    // ... more repositories
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

### 3. **Service Layer Pattern**
- Business logic separation
- DTOs for data transfer
- ServiceResult wrapper for consistent responses

```csharp
public interface IUserService
{
    Task<ServiceResult<UserDto>> GetUserByIdAsync(Guid userId);
    Task<ServiceResult<UserDto>> CreateUserAsync(CreateUserDto createUserDto);
    Task<ServiceResult<UserDto>> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto);
    // ... more methods
}
```

### 4. **Dependency Injection**
- All dependencies injected through constructor
- Configured in `Program.cs`
- Promotes loose coupling and testability

## 🗃️ Database Schema

The system manages a comprehensive pet care platform with the following modules:

### Core Modules:
1. **User Management**: Users, Roles, Authentication
2. **Pet Management**: Pets, Species, Breeds, Health Records, Vaccinations
3. **E-Commerce**: Products, Categories, Brands, Orders, Cart
4. **Service Booking**: Services, Appointments, Branches, Staff Schedules
5. **Blog & Community**: Blog Posts, Comments, Likes, Tags
6. **Chat & Support**: Chat Sessions, Messages, FAQ
7. **Reviews**: Product Reviews, Service Reviews
8. **Notifications**: User notifications

## 🔧 Configuration & Setup

### 1. Database Configuration

Update connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=petcare_db;Username=postgres;Password=your_password"
  }
}
```

### 2. Run Migrations

```bash
# Navigate to Infrastructure project
cd PetCare.Infrastructure

# Add migration
dotnet ef migrations add InitialCreate --startup-project ../PetCare.API

# Update database
dotnet ef database update --startup-project ../PetCare.API
```

### 3. Run the Application

```bash
cd PetCare.API
dotnet run
```

API will be available at: `https://localhost:5001`
Swagger UI: `https://localhost:5001/swagger`

## 📦 NuGet Packages Used

### Domain Layer
- No external dependencies (Pure POCO classes)

### Infrastructure Layer
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
```

### Application Layer
```xml
<PackageReference Include="AutoMapper" Version="13.0.1" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="13.0.1" />
```

### API Layer
```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
```

## 🚀 Key Features

### Generic Repository
- Supports complex queries with expressions
- Pagination support
- Include navigation properties
- Async operations

### AutoMapper Integration
- Automatic mapping between entities and DTOs
- Reduces boilerplate code
- Maintains separation of concerns

### Service Result Pattern
- Consistent API responses
- Success/Failure indication
- Error message handling
- Data payload

### Code-First Approach
- Entity models define database schema
- Fluent API for advanced configurations
- Automatic schema generation
- Migration support

## 📝 Usage Examples

### Creating a User

```csharp
// In Controller
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateUserDto createUserDto)
{
    var result = await _userService.CreateUserAsync(createUserDto);
    
    if (!result.Success)
    {
        return BadRequest(result);
    }
    
    return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
}

// In Service
public async Task<ServiceResult<UserDto>> CreateUserAsync(CreateUserDto createUserDto)
{
    // Check if email exists
    if (await _unitOfWork.Users.EmailExistsAsync(createUserDto.Email))
    {
        return ServiceResult<UserDto>.FailureResult("Email already exists");
    }

    // Map DTO to Entity
    var user = _mapper.Map<User>(createUserDto);
    
    // Save to database
    await _unitOfWork.Users.AddAsync(user);
    await _unitOfWork.SaveChangesAsync();

    // Map Entity to DTO and return
    var userDto = _mapper.Map<UserDto>(user);
    return ServiceResult<UserDto>.SuccessResult(userDto, "User created successfully");
}
```

### Getting Paginated Products

```csharp
[HttpGet]
public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
{
    var result = await _productService.GetProductsAsync(page, pageSize);
    return Ok(result);
}

// Returns:
{
  "success": true,
  "message": "Success",
  "data": {
    "items": [...],
    "totalCount": 100,
    "page": 1,
    "pageSize": 10,
    "totalPages": 10,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

## 🧪 Testing

The architecture is designed to be highly testable:

- **Repository Layer**: Can be mocked using `IGenericRepository<T>`
- **Service Layer**: Can be tested in isolation with mocked repositories
- **Controllers**: Can be tested with mocked services

Example test setup:

```csharp
// Mock repository
var mockUserRepository = new Mock<IUserRepository>();
var mockUnitOfWork = new Mock<IUnitOfWork>();
mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepository.Object);

// Mock mapper
var mockMapper = new Mock<IMapper>();

// Create service with mocked dependencies
var userService = new UserService(mockUnitOfWork.Object, mockMapper.Object);
```

## 📊 Database Naming Convention

The project uses **PostgreSQL snake_case** naming convention:
- Tables: `users`, `pets`, `products`
- Columns: `user_id`, `full_name`, `created_at`
- Schema: `PetCare`

This is configured in `PetCareDbContext` using Fluent API:

```csharp
entity.ToTable("users");
entity.Property(e => e.FullName).HasColumnName("full_name");
```

## 🔐 Security Considerations

1. **Row Level Security (RLS)**: Implement RLS policies in PostgreSQL
2. **Authentication**: Add JWT authentication
3. **Authorization**: Implement role-based authorization
4. **Input Validation**: Add FluentValidation
5. **SQL Injection**: Prevented by EF Core parameterization

## 🎨 Best Practices Implemented

1. ✅ **Separation of Concerns**: Each layer has distinct responsibilities
2. ✅ **DRY Principle**: Generic repository eliminates code duplication
3. ✅ **SOLID Principles**: Interfaces, dependency injection, single responsibility
4. ✅ **Async/Await**: All database operations are asynchronous
5. ✅ **Error Handling**: Consistent error handling with ServiceResult
6. ✅ **Code-First Migrations**: Database schema version control
7. ✅ **DTOs**: Prevents over-posting and separates internal/external models

## 🚧 Future Enhancements

- [ ] Add authentication (JWT)
- [ ] Add authorization policies
- [ ] Add FluentValidation
- [ ] Add caching (Redis)
- [ ] Add logging (Serilog)
- [ ] Add unit tests
- [ ] Add integration tests
- [ ] Add API versioning
- [ ] Add rate limiting
- [ ] Add health checks

## 📄 License

This project is licensed under the MIT License.

## 👥 Contributing

Contributions are welcome! Please follow the existing architecture patterns and coding conventions.

---

**Built with ❤️ for Pet Care Management**
