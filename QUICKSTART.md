# 🚀 Quick Start Guide - PetCare Platform

## Prerequisites

- .NET 8.0 SDK
- PostgreSQL 13+ 
- Visual Studio 2022 / VS Code / Rider
- Entity Framework Core CLI Tools

## Step-by-Step Setup

### 1. Clone/Setup the Project

```bash
cd F:\PetCare
```

### 2. Restore NuGet Packages

```bash
dotnet restore
```

### 3. Setup PostgreSQL Database

Create a new PostgreSQL database:

```sql
CREATE DATABASE petcare_db;
```

### 4. Update Connection String

Edit `PetCare.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=petcare_db;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

### 5. Install EF Core Tools (if not installed)

```bash
dotnet tool install --global dotnet-ef
```

### 6. Create and Apply Migrations

```bash
# From the solution root directory
dotnet ef migrations add InitialCreate --project PetCare.Infrastructure --startup-project PetCare.API

# Apply migration to database
dotnet ef database update --project PetCare.Infrastructure --startup-project PetCare.API
```

### 7. Run the Application

```bash
cd PetCare.API
dotnet run
```

Or press `F5` in Visual Studio.

### 8. Access the API

- **Swagger UI**: https://localhost:5001/swagger
- **API Base URL**: https://localhost:5001/api

## 📋 Testing the API

### Create a User

```bash
POST https://localhost:5001/api/users
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "fullName": "John Doe",
  "phone": "1234567890",
  "address": "123 Main St",
  "city": "Hanoi",
  "district": "Ba Dinh"
}
```

### Get All Users

```bash
GET https://localhost:5001/api/users?page=1&pageSize=10
```

### Create a Pet

```bash
POST https://localhost:5001/api/pets
Content-Type: application/json

{
  "userId": "user-guid-here",
  "petName": "Max",
  "gender": "Male",
  "weight": 5.5,
  "color": "Brown",
  "dateOfBirth": "2020-01-15"
}
```

### Get Products

```bash
GET https://localhost:5001/api/products?page=1&pageSize=20
```

### Search Products

```bash
GET https://localhost:5001/api/products/search?searchTerm=food
```

## 🏗️ Project Architecture Quick Reference

### Adding a New Entity

1. **Create Entity** in `PetCare.Domain/Entities/`
2. **Add DbSet** to `PetCareDbContext`
3. **Configure Entity** in DbContext (optional, for custom mappings)
4. **Create Migration**: `dotnet ef migrations add AddNewEntity`
5. **Update Database**: `dotnet ef database update`

### Adding a New Service

1. **Create DTO** in `PetCare.Application/DTOs/`
2. **Create Service Interface** in `Application/Services/Interfaces/`
3. **Implement Service** in `Application/Services/Implementations/`
4. **Add Mapping** in `MappingProfile.cs`
5. **Register Service** in `Program.cs`
6. **Create Controller** in `PetCare.API/Controllers/`

### Example: Adding a New Repository Method

```csharp
// 1. In IUserRepository interface
Task<IEnumerable<User>> GetUsersByCity(string city);

// 2. In UserRepository implementation
public async Task<IEnumerable<User>> GetUsersByCity(string city)
{
    return await _dbSet
        .Where(u => u.City == city)
        .ToListAsync();
}

// 3. Use in Service
var users = await _unitOfWork.Users.GetUsersByCity("Hanoi");
```

## 🔧 Common Commands

### Migrations

```bash
# Add new migration
dotnet ef migrations add MigrationName --project PetCare.Infrastructure --startup-project PetCare.API

# Update database
dotnet ef database update --project PetCare.Infrastructure --startup-project PetCare.API

# Rollback migration
dotnet ef database update PreviousMigrationName --project PetCare.Infrastructure --startup-project PetCare.API

# Remove last migration
dotnet ef migrations remove --project PetCare.Infrastructure --startup-project PetCare.API

# List all migrations
dotnet ef migrations list --project PetCare.Infrastructure --startup-project PetCare.API
```

### Build & Run

```bash
# Build solution
dotnet build

# Run API
dotnet run --project PetCare.API

# Run with watch (auto-reload)
dotnet watch run --project PetCare.API

# Clean build
dotnet clean
```

## 🐛 Troubleshooting

### Issue: Connection to PostgreSQL fails

**Solution**: 
- Check PostgreSQL is running
- Verify connection string
- Check firewall settings
- Ensure database exists

### Issue: Migration fails

**Solution**:
```bash
# Drop database and recreate
dotnet ef database drop --project PetCare.Infrastructure --startup-project PetCare.API
dotnet ef database update --project PetCare.Infrastructure --startup-project PetCare.API
```

### Issue: Port already in use

**Solution**: Change port in `launchSettings.json` or kill the process:
```bash
# Windows
netstat -ano | findstr :5001
taskkill /PID <PID> /F

# Linux/Mac
lsof -i :5001
kill -9 <PID>
```

## 📚 Additional Resources

- [EF Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [AutoMapper Documentation](https://docs.automapper.org/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)

## 💡 Tips

1. **Use Generic Repository**: For simple CRUD operations
2. **Create Specific Repository**: When you need custom queries
3. **Always use DTOs**: Never expose entities directly in API
4. **Use Unit of Work**: For transactions and consistency
5. **AutoMapper is your friend**: Let it handle object mapping
6. **Service Layer**: Put all business logic here, not in controllers

## 🎯 Next Steps

1. Add seed data for initial setup
2. Implement authentication (JWT)
3. Add validation (FluentValidation)
4. Add logging (Serilog)
5. Write unit tests
6. Add caching layer
7. Implement background jobs

---

**Happy Coding! 🚀**
