using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PetCare.Application.Common;
using PetCare.Application.Services;
using PetCare.Infrastructure.Data;
using PetCare.Infrastructure.Repositories.Interfaces;
using PetCare.Infrastructure.Repositories.Implementations;
using PetCare.Application.Services.Interfaces;
using PetCare.Application.Services.Implementations;

// Load environment variables from .env file
// Look for .env in the solution root (parent directory of PetCare.API)
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}
else
{
    // Try current directory
    DotNetEnv.Env.Load();
}

var builder = WebApplication.CreateBuilder(args);

// Override configuration with environment variables
builder.Configuration.AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "PetCare API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database configuration
var connectionString = Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING") 
    ?? builder.Configuration.GetConnectionString("SupabaseConnection");

// Debug output (remove in production)
Console.WriteLine($"Full connection string: {connectionString}");
if (!string.IsNullOrEmpty(connectionString))
{
    var passwordMatch = System.Text.RegularExpressions.Regex.Match(connectionString, @"Password=([^;]+)");
    if (passwordMatch.Success)
    {
        var password = passwordMatch.Groups[1].Value;
        Console.WriteLine($"Database password loaded: {password.Substring(0, Math.Min(4, password.Length))}... ({password.Length} chars)");
    }
    var portMatch = System.Text.RegularExpressions.Regex.Match(connectionString, @"Port=(\d+)");
    if (portMatch.Success)
    {
        Console.WriteLine($"Database port: {portMatch.Groups[1].Value}");
    }
}

builder.Services.AddDbContext<PetCareDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        b => {
            b.MigrationsAssembly("PetCare.Infrastructure");
            b.CommandTimeout(120); // 120 seconds timeout
        }
    ));

// AutoMapper configuration
builder.Services.AddAutoMapper(typeof(PetCare.Application.Mappings.MappingProfile));

// Register repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IBlogPostRepository, BlogPostRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IAIHealthService, AIHealthService>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<IServiceManagementService, ServiceManagementService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IHealthTrackingService, HealthTrackingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
// TODO: Uncomment when services are implemented
// builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<IChatService, ChatService>();
// builder.Services.AddScoped<IDashboardService, DashboardService>();

// Add HttpClient for AI services
builder.Services.AddHttpClient();

// Image upload service - Switch between Cloudinary and Local storage
var useCloudinary = Environment.GetEnvironmentVariable("USE_CLOUDINARY") == "true" 
    || builder.Configuration.GetValue<bool>("UseCloudinary", false);

if (useCloudinary)
{
    builder.Services.AddScoped<IImageUploadService, CloudinaryImageUploadService>();
    Console.WriteLine("Using Cloudinary for image uploads");
}
else
{
    builder.Services.AddScoped<IImageUploadService, LocalImageUploadService>();
    Console.WriteLine("Using Local storage for image uploads");
}

// Configure Image Upload Settings
builder.Services.Configure<ImageUploadSettings>(options =>
{
    options.StorageType = "Local";
    options.MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
    options.AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    options.LocalStoragePath = "wwwroot/uploads";
    options.BaseUrl = "/uploads";
});

builder.Services.Configure<JwtSettings>(options =>
{
    options.Key = Environment.GetEnvironmentVariable("JWT_KEY") 
        ?? builder.Configuration["Jwt:Key"] 
        ?? throw new InvalidOperationException("JWT Key not configured");
    options.Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
        ?? builder.Configuration["Jwt:Issuer"] 
        ?? "PetCare.API";
    options.Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
        ?? builder.Configuration["Jwt:Audience"] 
        ?? "PetCare.Client";
    
    var expiresMinutes = Environment.GetEnvironmentVariable("JWT_EXPIRES_MINUTES");
    options.ExpiresInMinutes = !string.IsNullOrEmpty(expiresMinutes) 
        ? int.Parse(expiresMinutes) 
        : builder.Configuration.GetValue<int>("Jwt:ExpiresInMinutes", 60);
});

var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") 
    ?? builder.Configuration["Jwt:Key"] 
    ?? throw new InvalidOperationException("JWT Key not configured");

if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key is empty. Please check your .env file.");
}

Console.WriteLine($"JWT Key loaded: {jwtKey.Length} characters");

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
    ?? builder.Configuration["Jwt:Issuer"];

var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
    ?? builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed database on startup (comment out after first successful run)
// Uncomment only when you need to re-seed the database
/*
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PetCareDbContext>();
    await DbInitializer.SeedAsync(context);
    
    // Seed from PetFinder API (only needed once)
    var httpClient = new HttpClient();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await DbInitializer.SeedFromPetFinderAsync(context, httpClient, configuration);
}
*/

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable serving static files from wwwroot
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
