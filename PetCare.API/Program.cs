using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PetCare.API.Services;
using PetCare.API.Security;
using PetCare.Application.Common;
using PetCare.Application.Services;
using PetCare.Infrastructure.Data;
using PetCare.Infrastructure.Repositories.Interfaces;
using PetCare.Infrastructure.Repositories.Implementations;
using PetCare.Application.Services.Interfaces;
using PetCare.Application.Services.Implementations;
using PetCare.Infrastructure.Services;
using PetCare.Domain.Interfaces;
using Npgsql;
using Resend;

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
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();
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

var runDbBootstrapOnStartup = GetBooleanSetting(
    "RUN_DB_BOOTSTRAP_ON_STARTUP",
    builder.Environment.IsDevelopment());

var disableDbUserSafetyCheck = GetBooleanSetting(
    "DISABLE_DB_USER_SAFETY_CHECK",
    false);

if (builder.Environment.IsProduction() && !disableDbUserSafetyCheck && IsPrivilegedDbUser(connectionString))
{
    throw new InvalidOperationException(
        "Unsafe DB configuration: production is using a privileged database account. " +
        "Use a least-privilege app user for SUPABASE_CONNECTION_STRING, or set " +
        "DISABLE_DB_USER_SAFETY_CHECK=true only as a temporary emergency override.");
}

if (builder.Environment.IsDevelopment())
{
    // Debug output (development only)
    Console.WriteLine($"Connection string configured: {!string.IsNullOrEmpty(connectionString)}");
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
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<IHealthRecordService, HealthRecordService>();
builder.Services.AddScoped<IAIHealthService, AIHealthService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddHostedService<VaccinationReminderHostedService>();

// Resend email service
var resendApiKey = Environment.GetEnvironmentVariable("RESEND_API_KEY")
    ?? builder.Configuration["Resend:ApiKey"]
    ?? string.Empty;
builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(options =>
{
    options.ApiToken = resendApiKey;
});
builder.Services.AddTransient<IResend, ResendClient>();
builder.Services.AddScoped<PetCare.Domain.Interfaces.IEmailService, ResendEmailService>();

// Add HttpClient for AI services
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("GeminiClient");

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

if (builder.Environment.IsDevelopment())
{
    Console.WriteLine($"JWT Key loaded: {jwtKey.Length} characters");
}

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

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var blacklist = context.HttpContext.RequestServices.GetRequiredService<ITokenBlacklistService>();

                var authHeader = context.Request.Headers.Authorization.ToString();
                var rawToken = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authHeader[7..].Trim()
                    : null;

                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                if (blacklist.IsBlacklisted(rawToken, jti))
                {
                    context.Fail("Token has been revoked");
                }

                return Task.CompletedTask;
            }
        };
    });

// CORS configuration - environment-based for security
builder.Services.AddCors(options =>
{
    var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")
        ?? builder.Configuration["AllowedOrigins"];
    
    options.AddPolicy("AppCorsPolicy", policy =>
    {
        if (string.IsNullOrEmpty(allowedOrigins) || allowedOrigins == "*")
        {
            // Allow all origins (development or when explicitly set to *)
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
            Console.WriteLine($"CORS: Allowing all origins ({builder.Environment.EnvironmentName} mode)");
        }
        else
        {
            // Production - specific origins only
            policy.WithOrigins(allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries))
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
            Console.WriteLine($"CORS: Allowing origins: {allowedOrigins}");
        }
    });
});

var app = builder.Build();

if (runDbBootstrapOnStartup)
{
    // Ensure core roles exist so role assignment and authorization policies work.
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<PetCareDbContext>();
            await context.Database.MigrateAsync();

            var requiredRoles = new[] { "Customer", "Doctor", "Admin", "Staff" };

            foreach (var roleName in requiredRoles)
            {
                var exists = await context.Roles.AnyAsync(r => r.RoleName == roleName);
                if (!exists)
                {
                    context.Roles.Add(new PetCare.Domain.Entities.Role
                    {
                        RoleName = roleName,
                        Description = $"Auto-generated role {roleName}"
                    });
                }
            }

            await context.SaveChangesAsync();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Startup warning: role initialization skipped due to database error: {ex.Message}");
        }
    }

    // Enable Row-Level Security on all tables in the petcare schema.
    // This is idempotent and keeps Supabase from exposing the "RLS is not enabled" warning.
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<PetCareDbContext>();
            var connection = context.Database.GetDbConnection();
            var shouldCloseConnection = connection.State != System.Data.ConnectionState.Open;

            if (shouldCloseConnection)
            {
                await connection.OpenAsync();
            }

            var tableNames = new List<string>();

            await using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT tablename FROM pg_tables WHERE schemaname = 'petcare' ORDER BY tablename;";
                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    tableNames.Add(reader.GetString(0));
                }
            }

            foreach (var tableName in tableNames)
            {
                await using var enableRlsCommand = connection.CreateCommand();
                enableRlsCommand.CommandText = $"ALTER TABLE petcare.\"{tableName.Replace("\"", "\"\"")}\" ENABLE ROW LEVEL SECURITY;";
                await enableRlsCommand.ExecuteNonQueryAsync();
            }

            await using (var policyCommand = connection.CreateCommand())
            {
                policyCommand.CommandText = @"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_policies
        WHERE schemaname = 'petcare' AND tablename = 'pet_species' AND policyname = 'pet_species_public_read'
    ) THEN
        CREATE POLICY pet_species_public_read ON petcare.pet_species
            FOR SELECT
            USING (true);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_policies
        WHERE schemaname = 'petcare' AND tablename = 'pet_breeds' AND policyname = 'pet_breeds_public_read'
    ) THEN
        CREATE POLICY pet_breeds_public_read ON petcare.pet_breeds
            FOR SELECT
            USING (true);
    END IF;
END $$;";
                await policyCommand.ExecuteNonQueryAsync();
            }

            var speciesCountCommand = connection.CreateCommand();
            speciesCountCommand.CommandText = "SELECT COUNT(*) FROM petcare.pet_species;";
            var speciesCount = Convert.ToInt32(await speciesCountCommand.ExecuteScalarAsync());

            var breedCountCommand = connection.CreateCommand();
            breedCountCommand.CommandText = "SELECT COUNT(*) FROM petcare.pet_breeds;";
            var breedCount = Convert.ToInt32(await breedCountCommand.ExecuteScalarAsync());

            Console.WriteLine($"Startup info: pet_species={speciesCount}, pet_breeds={breedCount}");

            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }

            Console.WriteLine($"Startup info: enabled RLS on {tableNames.Count} petcare tables.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Startup warning: RLS enablement skipped due to database error: {ex.Message}");
        }
    }
}
else
{
    Console.WriteLine("Startup info: DB bootstrap skipped (RUN_DB_BOOTSTRAP_ON_STARTUP=false).");
}

using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<PetCareDbContext>();
        var legacyMembershipPackages = await context.SubscriptionPackages
            .Where(p => p.IsActive && (
                p.Price == 5000m
                || p.Name == "5K"
                || (p.Description != null && p.Description.Contains("5.000"))))
            .ToListAsync();

        if (legacyMembershipPackages.Count > 0)
        {
            foreach (var package in legacyMembershipPackages)
            {
                package.Price = 30000m;

                if (package.Name == "5K")
                {
                    package.Name = "Premium";
                }

                if (!string.IsNullOrWhiteSpace(package.Description)
                    && package.Description.Contains("5.000", StringComparison.Ordinal))
                {
                    package.Description = package.Description.Replace("5.000", "30.000", StringComparison.Ordinal);
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"Startup info: synchronized {legacyMembershipPackages.Count} legacy membership package(s) to Premium/30k.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Startup warning: membership package sync skipped due to database error: {ex.Message}");
    }
}

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
// Enable Swagger in all environments (you can restrict later)
app.UseSwagger();
app.UseSwaggerUI();

// Enable serving static files from wwwroot
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseCors("AppCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static bool GetBooleanSetting(string key, bool defaultValue)
{
    var rawValue = Environment.GetEnvironmentVariable(key);
    if (string.IsNullOrWhiteSpace(rawValue))
    {
        return defaultValue;
    }

    if (bool.TryParse(rawValue, out var parsedBool))
    {
        return parsedBool;
    }

    if (rawValue == "1")
    {
        return true;
    }

    if (rawValue == "0")
    {
        return false;
    }

    return defaultValue;
}

static bool IsPrivilegedDbUser(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return false;
    }

    try
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var username = builder.Username?.Trim();

        if (string.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        return username.StartsWith("postgres", StringComparison.OrdinalIgnoreCase)
            || username.StartsWith("supabase_admin", StringComparison.OrdinalIgnoreCase)
            || username.Equals("admin", StringComparison.OrdinalIgnoreCase)
            || username.Equals("root", StringComparison.OrdinalIgnoreCase);
    }
    catch
    {
        return false;
    }
}
