using PetCare.Domain.Entities;

namespace PetCare.Infrastructure.Data;

/// <summary>
/// Seed data for Pet Species and Breeds
/// Data can be populated from public APIs like TheDogAPI.com and TheCatAPI.com
/// </summary>
public static class PetSpeciesSeedData
{
    public static List<PetSpecies> GetSpecies()
    {
        return new List<PetSpecies>
        {
            new PetSpecies
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                SpeciesName = "Dog",
                Description = "Domestic dog (Canis familiaris)",
                CreatedAt = DateTime.UtcNow
            },
            new PetSpecies
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                SpeciesName = "Cat",
                Description = "Domestic cat (Felis catus)",
                CreatedAt = DateTime.UtcNow
            },
            new PetSpecies
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                SpeciesName = "Bird",
                Description = "Various bird species commonly kept as pets",
                CreatedAt = DateTime.UtcNow
            },
            new PetSpecies
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                SpeciesName = "Rabbit",
                Description = "Domestic rabbit (Oryctolagus cuniculus)",
                CreatedAt = DateTime.UtcNow
            },
            new PetSpecies
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                SpeciesName = "Hamster",
                Description = "Various hamster species",
                CreatedAt = DateTime.UtcNow
            },
            new PetSpecies
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                SpeciesName = "Fish",
                Description = "Various aquarium fish species",
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    public static List<PetBreed> GetDogBreeds()
    {
        var dogSpeciesId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        
        return new List<PetBreed>
        {
            // Popular dog breeds - data structure based on TheDogAPI
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = dogSpeciesId, BreedName = "Golden Retriever", Characteristics = "Friendly, intelligent, devoted. Great family dog." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = dogSpeciesId, BreedName = "German Shepherd", Characteristics = "Confident, courageous, smart. Excellent working dog." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = dogSpeciesId, BreedName = "Labrador Retriever", Characteristics = "Outgoing, even-tempered, gentle. Most popular family dog." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = dogSpeciesId, BreedName = "French Bulldog", Characteristics = "Playful, adaptable, smart. Great for apartment living." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = dogSpeciesId, BreedName = "Bulldog", Characteristics = "Calm, courageous, friendly. Low exercise needs." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = dogSpeciesId, BreedName = "Poodle", Characteristics = "Intelligent, active, elegant. Comes in toy, miniature, and standard sizes." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = dogSpeciesId, BreedName = "Beagle", Characteristics = "Friendly, curious, merry. Great with children." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = dogSpeciesId, BreedName = "Rottweiler", Characteristics = "Loyal, loving, confident guardian." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = dogSpeciesId, BreedName = "Yorkshire Terrier", Characteristics = "Affectionate, sprightly, tomboyish. Small but feisty." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = dogSpeciesId, BreedName = "Boxer", Characteristics = "Fun-loving, bright, active. Patient with children." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = dogSpeciesId, BreedName = "Dachshund", Characteristics = "Clever, lively, courageous. Distinctive long body." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = dogSpeciesId, BreedName = "Siberian Husky", Characteristics = "Outgoing, mischievous, loyal. Loves cold weather." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = dogSpeciesId, BreedName = "Pomeranian", Characteristics = "Inquisitive, bold, lively. Fluffy toy breed." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = dogSpeciesId, BreedName = "Shih Tzu", Characteristics = "Affectionate, playful, outgoing. Ancient toy breed." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = dogSpeciesId, BreedName = "Chihuahua", Characteristics = "Charming, graceful, sassy. Smallest dog breed." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = dogSpeciesId, BreedName = "Mixed Breed", Characteristics = "Unique combination of breeds with varied characteristics." }
        };
    }

    public static List<PetBreed> GetCatBreeds()
    {
        var catSpeciesId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        
        return new List<PetBreed>
        {
            // Popular cat breeds - data structure based on TheCatAPI
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = catSpeciesId, BreedName = "Persian", Characteristics = "Quiet, sweet-tempered. Requires daily grooming." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = catSpeciesId, BreedName = "Maine Coon", Characteristics = "Gentle giant, friendly, good with children." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = catSpeciesId, BreedName = "Siamese", Characteristics = "Social, intelligent, vocal. Very affectionate." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = catSpeciesId, BreedName = "British Shorthair", Characteristics = "Easy-going, calm, affectionate. Round face." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = catSpeciesId, BreedName = "Ragdoll", Characteristics = "Docile, placid, affectionate. Goes limp when picked up." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = catSpeciesId, BreedName = "Scottish Fold", Characteristics = "Sweet-tempered, adaptable. Distinctive folded ears." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = catSpeciesId, BreedName = "Sphynx", Characteristics = "Energetic, loyal, dog-like. Hairless breed." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = catSpeciesId, BreedName = "Bengal", Characteristics = "Active, playful, energetic. Wild appearance." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = catSpeciesId, BreedName = "Russian Blue", Characteristics = "Quiet, gentle, loyal. Silvery-blue coat." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = catSpeciesId, BreedName = "Abyssinian", Characteristics = "Active, playful, curious. Loves to climb." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = catSpeciesId, BreedName = "Domestic Shorthair", Characteristics = "Mixed breed cat with varied characteristics. Most common." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = catSpeciesId, BreedName = "Domestic Longhair", Characteristics = "Mixed breed cat with long fur. Varied personalities." }
        };
    }

    public static List<PetBreed> GetOtherBreeds()
    {
        var birdSpeciesId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var rabbitSpeciesId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        
        return new List<PetBreed>
        {
            // Bird breeds
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = birdSpeciesId, BreedName = "Budgerigar (Budgie)", Characteristics = "Social, playful, easy to care for." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = birdSpeciesId, BreedName = "Cockatiel", Characteristics = "Friendly, affectionate, vocal." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = birdSpeciesId, BreedName = "African Grey Parrot", Characteristics = "Highly intelligent, excellent talker." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = birdSpeciesId, BreedName = "Canary", Characteristics = "Beautiful singer, low maintenance." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = birdSpeciesId, BreedName = "Lovebird", Characteristics = "Affectionate, social, colorful." },
            
            // Rabbit breeds
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = rabbitSpeciesId, BreedName = "Holland Lop", Characteristics = "Small, friendly, floppy ears." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = rabbitSpeciesId, BreedName = "Netherland Dwarf", Characteristics = "Tiny, energetic, compact." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = rabbitSpeciesId, BreedName = "Flemish Giant", Characteristics = "Very large, gentle, docile." },
            new PetBreed { Id = Guid.NewGuid(), SpeciesId = rabbitSpeciesId, BreedName = "Lionhead", Characteristics = "Friendly, furry mane around head." }
        };
    }

    public static List<PetBreed> GetAllBreeds()
    {
        var breeds = new List<PetBreed>();
        breeds.AddRange(GetDogBreeds());
        breeds.AddRange(GetCatBreeds());
        breeds.AddRange(GetOtherBreeds());
        return breeds;
    }
}
