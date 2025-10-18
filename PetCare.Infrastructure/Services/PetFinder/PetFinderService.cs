using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PetCare.Domain.Entities;

namespace PetCare.Infrastructure.Services.PetFinder;

/// <summary>
/// Service to fetch pet breeds from PetFinder API
/// Documentation: https://www.petfinder.com/developers/v2/docs/
/// </summary>
public class PetFinderService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private string? _accessToken;
    private DateTime _tokenExpiry;

    public PetFinderService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.petfinder.com/v2/");
        
        _apiKey = Environment.GetEnvironmentVariable("PETFINDER_API_KEY") 
            ?? configuration["PetFinder:ApiKey"] 
            ?? throw new InvalidOperationException("PetFinder API Key not configured");
            
        _apiSecret = Environment.GetEnvironmentVariable("PETFINDER_API_SECRET") 
            ?? configuration["PetFinder:ApiSecret"] 
            ?? throw new InvalidOperationException("PetFinder API Secret not configured");
    }

    /// <summary>
    /// Get OAuth access token from PetFinder
    /// </summary>
    private async Task<string> GetAccessTokenAsync()
    {
        // Return cached token if still valid
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
        {
            return _accessToken;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "oauth2/token");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", _apiKey },
            { "client_secret", _apiSecret }
        });

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<PetFinderAuthResponse>();
        
        if (authResponse == null)
        {
            throw new Exception("Failed to get access token from PetFinder");
        }

        _accessToken = authResponse.access_token;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(authResponse.expires_in - 60); // Refresh 1 min early
        
        return _accessToken;
    }

    /// <summary>
    /// Get all animal types (species) from PetFinder
    /// </summary>
    public async Task<List<PetSpecies>> GetAnimalTypesAsync()
    {
        var token = await GetAccessTokenAsync();
        
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var response = await _httpClient.GetAsync("types");
        response.EnsureSuccessStatusCode();

        var typesResponse = await response.Content.ReadFromJsonAsync<PetFinderTypesResponse>();
        
        if (typesResponse == null || typesResponse.types == null)
        {
            return new List<PetSpecies>();
        }

        var species = new List<PetSpecies>();
        
        foreach (var type in typesResponse.types)
        {
            species.Add(new PetSpecies
            {
                Id = Guid.NewGuid(),
                SpeciesName = type.name,
                Description = $"{type.name} - various breeds and characteristics",
                CreatedAt = DateTime.UtcNow
            });
        }

        return species;
    }

    /// <summary>
    /// Get breeds for a specific animal type
    /// </summary>
    public async Task<List<PetBreed>> GetBreedsAsync(string animalType, Guid speciesId)
    {
        var token = await GetAccessTokenAsync();
        
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var response = await _httpClient.GetAsync($"types/{animalType.ToLower()}/breeds");
        
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to fetch breeds for {animalType}: {response.StatusCode}");
            return new List<PetBreed>();
        }

        var breedsResponse = await response.Content.ReadFromJsonAsync<PetFinderBreedsResponse>();
        
        if (breedsResponse == null || breedsResponse.breeds == null)
        {
            return new List<PetBreed>();
        }

        var breeds = new List<PetBreed>();
        
        foreach (var breed in breedsResponse.breeds)
        {
            breeds.Add(new PetBreed
            {
                Id = Guid.NewGuid(),
                SpeciesId = speciesId,
                BreedName = breed.name,
                Characteristics = $"{breed.name} - {animalType}",
                CreatedAt = DateTime.UtcNow
            });
        }

        return breeds;
    }

    /// <summary>
    /// Get all species and their breeds from PetFinder
    /// </summary>
    public async Task<(List<PetSpecies> Species, List<PetBreed> Breeds)> GetAllSpeciesAndBreedsAsync()
    {
        Console.WriteLine("Fetching animal types from PetFinder API...");
        var species = await GetAnimalTypesAsync();
        
        Console.WriteLine($"✓ Found {species.Count} animal types");
        
        var allBreeds = new List<PetBreed>();
        
        foreach (var sp in species)
        {
            Console.WriteLine($"Fetching breeds for {sp.SpeciesName}...");
            
            try
            {
                var breeds = await GetBreedsAsync(sp.SpeciesName, sp.Id);
                allBreeds.AddRange(breeds);
                Console.WriteLine($"  ✓ Found {breeds.Count} breeds");
                
                // Rate limiting - be nice to the API
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Error fetching breeds for {sp.SpeciesName}: {ex.Message}");
            }
        }
        
        Console.WriteLine($"✓ Total: {species.Count} species, {allBreeds.Count} breeds");
        
        return (species, allBreeds);
    }
}
