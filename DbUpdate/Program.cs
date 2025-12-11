using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PetCare.Infrastructure.Data;

namespace PetCare.DbUpdate;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting database update to third-party service model...");
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("../PetCare.API/appsettings.json", optional: false)
            .AddJsonFile("../PetCare.API/appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("SupabaseConnection");
        
        var optionsBuilder = new DbContextOptionsBuilder<PetCareDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        
        await using var context = new PetCareDbContext(optionsBuilder.Options);
        
        // Read SQL script
        var sqlScript = await File.ReadAllTextAsync("../update_to_third_party_model.sql");
        
        try
        {
            // Execute SQL script
            await context.Database.ExecuteSqlRawAsync(sqlScript);
            Console.WriteLine("✅ Database updated successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error updating database: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
