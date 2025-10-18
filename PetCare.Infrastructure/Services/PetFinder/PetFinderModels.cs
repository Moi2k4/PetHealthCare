namespace PetCare.Infrastructure.Services.PetFinder;

/// <summary>
/// PetFinder API response models
/// Based on https://www.petfinder.com/developers/v2/docs/
/// </summary>

public class PetFinderAuthResponse
{
    public string token_type { get; set; } = string.Empty;
    public int expires_in { get; set; }
    public string access_token { get; set; } = string.Empty;
}

public class PetFinderBreedsResponse
{
    public List<PetFinderBreed> breeds { get; set; } = new();
}

public class PetFinderBreed
{
    public string name { get; set; } = string.Empty;
}

public class PetFinderTypesResponse
{
    public List<PetFinderType> types { get; set; } = new();
}

public class PetFinderType
{
    public string name { get; set; } = string.Empty;
    public List<string> coats { get; set; } = new();
    public List<string> colors { get; set; } = new();
    public List<string> genders { get; set; } = new();
}
