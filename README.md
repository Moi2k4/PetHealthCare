# PetCare - Pet Healthcare Management Platform# PetCare Platform - Clean Architecture with Code-First Approach



> A comprehensive web platform for pet care management with e-commerce, appointment booking, health tracking, and community features.## 🏗️ Architecture Overview



[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)This project implements a **Clean Architecture** pattern with **Code-First approach** using:

[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Supabase-336791)](https://supabase.com/)- **Domain Layer**: Entity models

[![License](https://img.shields.io/badge/license-Private-red)]()- **Infrastructure Layer**: DbContext, Repositories, Data Access

- **Application Layer**: Services, DTOs, AutoMapper profiles

## 🚀 Quick Start- **API Layer**: Controllers, API endpoints



### 1. Clone & Setup## 📁 Project Structure

```bash

git clone https://github.com/Moi2k4/PetHealthCare.git```

cd PetHealthCarePetCare/

├── PetCare.Domain/              # Domain Layer

# Copy environment template│   ├── Common/

copy .env.example .env│   │   ├── BaseEntity.cs        # Base entity with Id, CreatedAt

│   │   └── AuditableEntity.cs   # Base entity with UpdatedAt

# Edit with your credentials│   └── Entities/                # Domain entities

notepad .env│       ├── User.cs

```│       ├── Pet.cs

│       ├── Product.cs

### 2. Configure Environment│       ├── Order.cs

Edit `.env` with your actual credentials:│       ├── Appointment.cs

```env│       └── ...

SUPABASE_CONNECTION_STRING=your_connection_string_here│

JWT_KEY=your_secure_jwt_key_minimum_32_characters├── PetCare.Infrastructure/      # Infrastructure Layer

```│   ├── Data/

│   │   └── PetCareDbContext.cs  # EF Core DbContext

### 3. Run Database Migrations│   └── Repositories/

```bash│       ├── Interfaces/          # Repository interfaces

cd PetCare.Infrastructure│       │   ├── IGenericRepository.cs

dotnet ef database update --startup-project ..\PetCare.API│       │   ├── IUnitOfWork.cs

```│       │   ├── IUserRepository.cs

│       │   └── ...

### 4. Start the Application│       └── Implementations/     # Repository implementations

```bash│           ├── GenericRepository.cs

cd ..\PetCare.API│           ├── UnitOfWork.cs

dotnet run│           ├── UserRepository.cs

```│           └── ...

│

### 5. Access the API├── PetCare.Application/         # Application Layer

- **Swagger UI:** https://localhost:54813/swagger│   ├── DTOs/                    # Data Transfer Objects

- **API Base:** https://localhost:54813/api│   │   ├── User/

│   │   ├── Pet/

---│   │   ├── Product/

│   │   └── ...

## 📋 Features│   ├── Mappings/

│   │   └── MappingProfile.cs    # AutoMapper profiles

### ✅ Implemented│   ├── Services/

│   │   ├── Interfaces/          # Service interfaces

#### 🔐 Authentication & Authorization│   │   │   ├── IUserService.cs

- JWT-based authentication with BCrypt password hashing│   │   │   ├── IPetService.cs

- Role-based access control (Admin, Staff, Doctor, Customer)│   │   │   └── ...

- User registration (auto-assigned Customer role)│   │   └── Implementations/     # Service implementations

- Admin-only role management endpoint│   │       ├── UserService.cs

- Secure login with token generation│   │       ├── PetService.cs

│   │       └── ...

#### 🛒 E-Commerce (Shopping Cart & Orders)│   └── Common/

- **Cart Management:**│       ├── ServiceResult.cs     # Service response wrapper

  - Add/update/remove products│       └── PagedResult.cs       # Pagination wrapper

  - Real-time stock validation│

  - Automatic quantity updates└── PetCare.API/                 # API Layer

  - Cart total calculation    ├── Controllers/

  - Sale price support    │   ├── UsersController.cs

    │   ├── PetsController.cs

- **Order Processing:**    │   └── ProductsController.cs

  - Create orders from cart    ├── Program.cs               # Application configuration

  - Transaction support (atomic operations)    └── appsettings.json         # Configuration settings

  - Automatic stock management```

  - Order status tracking (Pending, Confirmed, Cancelled, etc.)

  - User order history## 🎯 Design Patterns Implemented

  - Admin order management

  - Shipping fee calculation (free over 500k VND)### 1. **Repository Pattern**

  - Cancel orders (Pending/Confirmed only)- Generic repository for common CRUD operations

- Specific repositories for entity-specific queries

#### 📦 Product Catalog- Abstraction over data access layer

- Product listing with pagination

- Category and brand filtering```csharp

- Product searchpublic interface IGenericRepository<T> where T : class

- Stock management{

- Multiple product images    Task<T?> GetByIdAsync(Guid id);

- Active/inactive products    Task<IEnumerable<T>> GetAllAsync();

    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

###  Planned Features    Task<T> AddAsync(T entity);

    Task UpdateAsync(T entity);

| Feature | Status | Description |    Task DeleteAsync(T entity);

|---------|--------|-------------|    // ... more methods

| 📅 Appointment Booking | Planned | Book vet visits, grooming, home service |}

| 🏥 Pet Health Tracking | Planned | Medical records, vaccinations, reminders |```

| 📝 Blog System | Planned | Articles, comments, likes, moderation |

| 💳 Payment Integration | Planned | MOMO & VNPay gateways |### 2. **Unit of Work Pattern**

| 🤖 AI Chatbot | Planned | Health consultation, recommendations |- Manages transactions across multiple repositories

- Ensures data consistency

---- Single point to save all changes



## 🏗️ Architecture```csharp

public interface IUnitOfWork : IDisposable

### Clean Architecture with Code-First Approach{

    IUserRepository Users { get; }

```    IPetRepository Pets { get; }

PetCare/    IProductRepository Products { get; }

├── PetCare.API/                 # 🎯 API Layer    // ... more repositories

│   ├── Controllers/             # REST endpoints    

│   │   ├── AuthController.cs    # Registration, login    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

│   │   ├── UsersController.cs   # User management (admin)    Task BeginTransactionAsync();

│   │   ├── CartController.cs    # Shopping cart    Task CommitTransactionAsync();

│   │   ├── OrdersController.cs  # Order management    Task RollbackTransactionAsync();

│   │   └── ProductsController.cs # Product catalog}

│   └── Program.cs               # App configuration + .env loading```

│

├── PetCare.Application/         # 💼 Business Logic Layer### 3. **Service Layer Pattern**

│   ├── DTOs/                    # Data transfer objects- Business logic separation

│   │   ├── Auth/                # Login, register DTOs- DTOs for data transfer

│   │   ├── User/                # User management DTOs- ServiceResult wrapper for consistent responses

│   │   ├── Product/             # Product & cart DTOs

│   │   └── Order/               # Order DTOs```csharp

│   ├── Services/                # Business logicpublic interface IUserService

│   │   ├── Interfaces/{

│   │   └── Implementations/    Task<ServiceResult<UserDto>> GetUserByIdAsync(Guid userId);

│   │       ├── AuthService.cs   # Auth logic    Task<ServiceResult<UserDto>> CreateUserAsync(CreateUserDto createUserDto);

│   │       ├── UserService.cs   # User management    Task<ServiceResult<UserDto>> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto);

│   │       ├── CartService.cs   # Cart operations    // ... more methods

│   │       └── OrderService.cs  # Order processing}

│   └── Mappings/                # AutoMapper profiles```

│

├── PetCare.Domain/              # 🎨 Domain Layer### 4. **Dependency Injection**

│   ├── Common/- All dependencies injected through constructor

│   │   ├── BaseEntity.cs        # Id, CreatedAt- Configured in `Program.cs`

│   │   └── AuditableEntity.cs   # UpdatedAt- Promotes loose coupling and testability

│   └── Entities/                # Domain models

│       ├── User.cs              # Users table## 🗃️ Database Schema

│       ├── Role.cs              # Roles table

│       ├── Product.cs           # Products tableThe system manages a comprehensive pet care platform with the following modules:

│       ├── CartItem.cs          # Cart items table

│       ├── Order.cs             # Orders table### Core Modules:

│       └── ... (30+ entities)1. **User Management**: Users, Roles, Authentication

│2. **Pet Management**: Pets, Species, Breeds, Health Records, Vaccinations

└── PetCare.Infrastructure/      # 🗄️ Data Access Layer3. **E-Commerce**: Products, Categories, Brands, Orders, Cart

    ├── Data/4. **Service Booking**: Services, Appointments, Branches, Staff Schedules

    │   └── PetCareDbContext.cs  # EF Core context5. **Blog & Community**: Blog Posts, Comments, Likes, Tags

    ├── Repositories/6. **Chat & Support**: Chat Sessions, Messages, FAQ

    │   ├── Interfaces/          # Repository contracts7. **Reviews**: Product Reviews, Service Reviews

    │   └── Implementations/     # Data access logic8. **Notifications**: User notifications

    └── Migrations/              # EF Core migrations

```## 🔧 Configuration & Setup



### Design Patterns### 1. Database Configuration

- ✅ **Repository Pattern** - Data access abstraction

- ✅ **Unit of Work Pattern** - Transaction managementUpdate connection string in `appsettings.json`:

- ✅ **Service Layer Pattern** - Business logic separation

- ✅ **Dependency Injection** - Loose coupling```json

- ✅ **DTO Pattern** - Data transfer & validation{

  "ConnectionStrings": {

---    "DefaultConnection": "Host=localhost;Port=5432;Database=petcare_db;Username=postgres;Password=your_password"

  }

## 🔧 Technology Stack}

```

| Layer | Technologies |

|-------|-------------|### 2. Run Migrations

| **Backend** | .NET 8 Web API |

| **Database** | PostgreSQL (Supabase) |```bash

| **ORM** | Entity Framework Core 8 |# Navigate to Infrastructure project

| **Authentication** | JWT Bearer Tokens |cd PetCare.Infrastructure

| **Password** | BCrypt.Net (hash + salt) |

| **Mapping** | AutoMapper |# Add migration

| **Documentation** | Swagger/OpenAPI |dotnet ef migrations add InitialCreate --startup-project ../PetCare.API

| **Config** | DotNetEnv (.env files) |

# Update database

---dotnet ef database update --startup-project ../PetCare.API

```

## 📚 API Documentation

### 3. Run the Application

### 🔐 Authentication (`/api/Auth`)

```bash

| Method | Endpoint | Description | Auth |cd PetCare.API

|--------|----------|-------------|------|dotnet run

| POST | `/register` | Register new user (Customer role) | Public |```

| POST | `/login` | Login & get JWT token | Public |

API will be available at: `https://localhost:5001`

**Example: Register**Swagger UI: `https://localhost:5001/swagger`

```json

POST /api/Auth/register## 📦 NuGet Packages Used

{

  "email": "user@example.com",### Domain Layer

  "password": "SecurePass123!",- No external dependencies (Pure POCO classes)

  "fullName": "John Doe",

  "phone": "0123456789"### Infrastructure Layer

}```xml

```<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />

<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />

**Example: Login**<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />

```json```

POST /api/Auth/login

{### Application Layer

  "email": "user@example.com",```xml

  "password": "SecurePass123!"<PackageReference Include="AutoMapper" Version="13.0.1" />

}<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="13.0.1" />

```

Response:

{### API Layer

  "success": true,```xml

  "data": {<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />

    "token": "eyJhbGc...",<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />

    "expiresAt": "2025-10-12T10:00:00Z",```

    "user": { ... }

  }## 🚀 Key Features

}

```### Generic Repository

- Supports complex queries with expressions

### 👤 User Management (`/api/Users`) - Admin Only- Pagination support

- Include navigation properties

| Method | Endpoint | Description | Auth |- Async operations

|--------|----------|-------------|------|

| GET | `/` | Get all users (paginated) | Admin |### AutoMapper Integration

| GET | `/{id}` | Get user by ID | Admin |- Automatic mapping between entities and DTOs

| POST | `/` | Create user | Admin |- Reduces boilerplate code

| PUT | `/{id}` | Update user | Admin |- Maintains separation of concerns

| PUT | `/{id}/role` | Update user role | Admin |

| DELETE | `/{id}` | Delete user | Admin |### Service Result Pattern

- Consistent API responses

**Example: Set User Role**- Success/Failure indication

```json- Error message handling

PUT /api/Users/{userId}/role- Data payload

Authorization: Bearer {admin-token}

### Code-First Approach

{- Entity models define database schema

  "roleName": "staff"- Fluent API for advanced configurations

  // or- Automatic schema generation

  "roleId": "460cda60-63b5-4047-afa2-91897225d1ec"- Migration support

}

```## 📝 Usage Examples



### 🛒 Shopping Cart (`/api/Cart`) - Authenticated### Creating a User



| Method | Endpoint | Description |```csharp

|--------|----------|-------------|// In Controller

| GET | `/` | Get cart items |[HttpPost]

| GET | `/total` | Get cart total |public async Task<IActionResult> Create([FromBody] CreateUserDto createUserDto)

| POST | `/` | Add product to cart |{

| PUT | `/{cartItemId}` | Update quantity |    var result = await _userService.CreateUserAsync(createUserDto);

| DELETE | `/{cartItemId}` | Remove item |    

| DELETE | `/` | Clear cart |    if (!result.Success)

    {

**Example: Add to Cart**        return BadRequest(result);

```json    }

POST /api/Cart    

Authorization: Bearer {token}    return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);

}

{

  "productId": "abc123...",// In Service

  "quantity": 2public async Task<ServiceResult<UserDto>> CreateUserAsync(CreateUserDto createUserDto)

}{

```    // Check if email exists

    if (await _unitOfWork.Users.EmailExistsAsync(createUserDto.Email))

### 📦 Orders (`/api/Orders`) - Authenticated    {

        return ServiceResult<UserDto>.FailureResult("Email already exists");

| Method | Endpoint | Description | Auth |    }

|--------|----------|-------------|------|

| GET | `/my-orders` | Get user's orders | User |    // Map DTO to Entity

| GET | `/{id}` | Get order details | User |    var user = _mapper.Map<User>(createUserDto);

| POST | `/` | Create order from cart | User |    

| POST | `/{id}/cancel` | Cancel order | User |    // Save to database

| GET | `/` | Get all orders | Admin |    await _unitOfWork.Users.AddAsync(user);

| PUT | `/{id}/status` | Update order status | Admin/Staff |    await _unitOfWork.SaveChangesAsync();



**Example: Create Order**    // Map Entity to DTO and return

```json    var userDto = _mapper.Map<UserDto>(user);

POST /api/Orders    return ServiceResult<UserDto>.SuccessResult(userDto, "User created successfully");

Authorization: Bearer {token}}

```

{

  "shippingName": "John Doe",### Getting Paginated Products

  "shippingPhone": "0123456789",

  "shippingAddress": "123 Main St, District 1",```csharp

  "paymentMethod": "COD",[HttpGet]

  "items": [public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)

    {{

      "productId": "product-guid",    var result = await _productService.GetProductsAsync(page, pageSize);

      "quantity": 2    return Ok(result);

    }}

  ]

}// Returns:

```{

  "success": true,

### 🏷️ Products (`/api/Products`) - Public  "message": "Success",

  "data": {

| Method | Endpoint | Description |    "items": [...],

|--------|----------|-------------|    "totalCount": 100,

| GET | `/` | Get products (paginated) |    "page": 1,

| GET | `/{id}` | Get product by ID |    "pageSize": 10,

| GET | `/category/{categoryId}` | Filter by category |    "totalPages": 10,

| GET | `/search?searchTerm=food` | Search products |    "hasPreviousPage": false,

| GET | `/active` | Get active products |    "hasNextPage": true

  }

---}

```

## 🔐 Security

## 🧪 Testing

### Environment Variables (.env)

This project uses `.env` files for sensitive configuration:The architecture is designed to be highly testable:



✅ **Benefits:**- **Repository Layer**: Can be mocked using `IGenericRepository<T>`

- Single source for all secrets- **Service Layer**: Can be tested in isolation with mocked repositories

- Never committed to Git (gitignored)- **Controllers**: Can be tested with mocked services

- Works across environments

- Industry standardExample test setup:

- Easy team collaboration via `.env.example`

```csharp

✅ **Setup:**// Mock repository

```bashvar mockUserRepository = new Mock<IUserRepository>();

# 1. Copy templatevar mockUnitOfWork = new Mock<IUnitOfWork>();

copy .env.example .envmockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepository.Object);



# 2. Edit with real values// Mock mapper

notepad .envvar mockMapper = new Mock<IMapper>();

```

// Create service with mocked dependencies

✅ **Required Variables:**var userService = new UserService(mockUnitOfWork.Object, mockMapper.Object);

```env```

SUPABASE_CONNECTION_STRING=Host=...;Password=xxx

JWT_KEY=your_secret_key_32_chars_minimum## 📊 Database Naming Convention

JWT_ISSUER=PetCare.API

JWT_AUDIENCE=PetCare.ClientThe project uses **PostgreSQL snake_case** naming convention:

JWT_EXPIRES_MINUTES=60- Tables: `users`, `pets`, `products`

```- Columns: `user_id`, `full_name`, `created_at`

- Schema: `PetCare`

📖 **See [ENV_SETUP_GUIDE.md](ENV_SETUP_GUIDE.md) for complete documentation**

This is configured in `PetCareDbContext` using Fluent API:

### Authentication Flow

1. User registers → Password hashed with BCrypt → Customer role assigned```csharp

2. User logs in → Credentials verified → JWT token issuedentity.ToTable("users");

3. Client sends token in `Authorization: Bearer {token}` headerentity.Property(e => e.FullName).HasColumnName("full_name");

4. API validates token → Extracts user claims → Authorizes request```



### Authorization Levels## 🔐 Security Considerations

| Role | Access |

|------|--------|1. **Row Level Security (RLS)**: Implement RLS policies in PostgreSQL

| **Customer** | Cart, Orders, Own Profile |2. **Authentication**: Add JWT authentication

| **Staff** | + Order Management |3. **Authorization**: Implement role-based authorization

| **Doctor** | + Appointment Management |4. **Input Validation**: Add FluentValidation

| **Admin** | + User Management, Role Assignment |5. **SQL Injection**: Prevented by EF Core parameterization



---## 🎨 Best Practices Implemented



## 🗃️ Database1. ✅ **Separation of Concerns**: Each layer has distinct responsibilities

2. ✅ **DRY Principle**: Generic repository eliminates code duplication

### Schema: `petcare`3. ✅ **SOLID Principles**: Interfaces, dependency injection, single responsibility

4. ✅ **Async/Await**: All database operations are asynchronous

**Key Tables:**5. ✅ **Error Handling**: Consistent error handling with ServiceResult

- `users` - User accounts6. ✅ **Code-First Migrations**: Database schema version control

- `roles` - User roles (admin, staff, doctor, user)7. ✅ **DTOs**: Prevents over-posting and separates internal/external models

- `products` - Product catalog

- `product_categories` - Categories## 🚧 Future Enhancements

- `brands` - Product brands

- `cart_items` - Shopping cart- [ ] Add authentication (JWT)

- `orders` - Order records- [ ] Add authorization policies

- `order_items` - Order line items- [ ] Add FluentValidation

- `pets` - User's pets- [ ] Add caching (Redis)

- `appointments` - Service bookings- [ ] Add logging (Serilog)

- `blog_posts` - Articles- [ ] Add unit tests

- ... (30+ tables)- [ ] Add integration tests

- [ ] Add API versioning

**Naming Convention:**- [ ] Add rate limiting

- Tables: `snake_case` (e.g., `product_categories`)- [ ] Add health checks

- Columns: `snake_case` (e.g., `full_name`, `created_at`)

- Primary Keys: `id` (UUID)## 📄 License



### MigrationsThis project is licensed under the MIT License.



```bash## 👥 Contributing

# Add migration

cd PetCare.InfrastructureContributions are welcome! Please follow the existing architecture patterns and coding conventions.

dotnet ef migrations add MigrationName --startup-project ../PetCare.API

---

# Apply migration

dotnet ef database update --startup-project ../PetCare.API**Built with ❤️ for Pet Care Management**


# Rollback last migration
dotnet ef migrations remove --startup-project ../PetCare.API
```

---

## 🛠️ Development

### Prerequisites
- .NET 8 SDK
- PostgreSQL (Supabase account)
- Git
- Visual Studio 2022 / VS Code / Rider

### Build & Run

```bash
# Restore packages
dotnet restore

# Build
dotnet build PetCare.sln

# Run
cd PetCare.API
dotnet run

# Watch mode (auto-reload)
dotnet watch run

# Clean build
dotnet clean
dotnet build
```

### Project Commands

```bash
# Add package to specific project
cd PetCare.Application
dotnet add package PackageName

# List references
dotnet list reference

# Run tests (when implemented)
dotnet test
```

---

## 📂 File Structure

```
PetCare/
├── .env                         # Your secrets (gitignored)
├── .env.example                 # Template for team
├── .gitignore                   # Git exclusions
├── PetCare.sln                  # Solution file
├── README.md                    # This file
├── ENV_SETUP_GUIDE.md           # Environment setup docs
└── SHOPPING_CART_ORDER_IMPLEMENTATION.md  # Feature docs
```

---

## 👥 Team Collaboration

### For New Team Members:

1. **Clone repository**
   ```bash
   git clone https://github.com/Moi2k4/PetHealthCare.git
   cd PetHealthCare
   ```

2. **Setup environment**
   ```bash
   copy .env.example .env
   # Ask team lead for actual credentials
   notepad .env
   ```

3. **Run migrations**
   ```bash
   cd PetCare.Infrastructure
   dotnet ef database update --startup-project ../PetCare.API
   ```

4. **Start developing**
   ```bash
   cd ../PetCare.API
   dotnet run
   ```

### Sharing Secrets
✅ **Do:** Use password managers, encrypted vaults
❌ **Don't:** Email, Slack, Git commits, screenshots

---

## 🚧 Roadmap

### Phase 1: Foundation ✅
- [x] Clean architecture setup
- [x] Database schema & migrations
- [x] Authentication & authorization
- [x] User management
- [x] Shopping cart & orders

### Phase 2: Core Features (In Progress)
- [ ] Appointment booking system
- [ ] Pet health tracking
- [ ] Blog system with moderation
- [ ] Payment gateway integration (MOMO, VNPay)

### Phase 3: Advanced Features
- [ ] AI chatbot for pet health
- [ ] Real-time chat support
- [ ] Mobile app (React Native)
- [ ] Admin dashboard
- [ ] Analytics & reporting

### Phase 4: Optimization
- [ ] Caching (Redis)
- [ ] CDN for images
- [ ] Background jobs (Hangfire)
- [ ] Performance monitoring
- [ ] Load testing

---

## 📄 License

This project is **private and proprietary**.

---

## 📞 Contact & Support

- **Developer:** Moi2k4
- **Repository:** https://github.com/Moi2k4/PetHealthCare
- **Issues:** [Create an issue](https://github.com/Moi2k4/PetHealthCare/issues)

---

## 🎯 Key Highlights

- ✅ Clean architecture with clear separation of concerns
- ✅ Secure authentication with JWT & BCrypt
- ✅ Environment-based configuration (.env)
- ✅ Transaction support for data integrity
- ✅ Comprehensive API documentation (Swagger)
- ✅ Role-based authorization
- ✅ Repository + Unit of Work patterns
- ✅ AutoMapper for object mapping
- ✅ Async/await throughout
- ✅ PostgreSQL with EF Core migrations

---

**Built with ❤️ for Pet Healthcare Management**

*"Because every pet deserves the best care"* 🐾
