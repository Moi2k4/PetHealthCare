using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Services.PetFinder;

namespace PetCare.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(PetCareDbContext context)
    {
        try
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (await context.Roles.AnyAsync())
            {
                return; // Database has been seeded
            }

            // Seed Roles
            var roles = new List<Role>
            {
                new Role { RoleName = "admin", Description = "Quản trị viên hệ thống" },
                new Role { RoleName = "service_provider", Description = "Nhà cung cấp dịch vụ (Grooming, Pet Hotel, etc.)" },
                new Role { RoleName = "staff", Description = "Nhân viên chăm sóc/grooming" },
                new Role { RoleName = "user", Description = "Người dùng thông thường" }
            };
            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();

            // Seed Pet Species using comprehensive data
            var species = PetSpeciesSeedData.GetSpecies();
            await context.PetSpecies.AddRangeAsync(species);
            await context.SaveChangesAsync();
            Console.WriteLine($"✓ Seeded {species.Count} pet species");

            // Seed Pet Breeds (comprehensive list from seed data)
            var breeds = PetSpeciesSeedData.GetAllBreeds();
            await context.PetBreeds.AddRangeAsync(breeds);
            await context.SaveChangesAsync();
            Console.WriteLine($"✓ Seeded {breeds.Count} pet breeds");

            // Seed Service Categories
            var serviceCategories = new List<ServiceCategory>
            {
                new ServiceCategory { CategoryName = "Grooming", Description = "Dịch vụ cắt tỉa, tắm rửa", IconUrl = "/icons/grooming.svg" },
                new ServiceCategory { CategoryName = "Spa & Chăm sóc", Description = "Dịch vụ spa và chăm sóc sắc đẹp", IconUrl = "/icons/spa.svg" },
                new ServiceCategory { CategoryName = "Khách sạn thú cưng", Description = "Dịch vụ lưu trú thú cưng", IconUrl = "/icons/hotel.svg" },
                new ServiceCategory { CategoryName = "Huấn luyện", Description = "Dịch vụ huấn luyện thú cưng", IconUrl = "/icons/training.svg" },
                new ServiceCategory { CategoryName = "Tư vấn sức khỏe", Description = "Tư vấn và giới thiệu dịch vụ thú y đối tác", IconUrl = "/icons/consultation.svg" },
                new ServiceCategory { CategoryName = "Dịch vụ tại nhà", Description = "Các dịch vụ chăm sóc tại nhà", IconUrl = "/icons/home-service.svg" }
            };
            await context.ServiceCategories.AddRangeAsync(serviceCategories);
            await context.SaveChangesAsync();

            // Seed Services
            var groomingCategory = serviceCategories.First(sc => sc.CategoryName == "Grooming");
            var spaCategory = serviceCategories.First(sc => sc.CategoryName == "Spa & Chăm sóc");
            var hotelCategory = serviceCategories.First(sc => sc.CategoryName == "Khách sạn thú cưng");
            var consultationCategory = serviceCategories.First(sc => sc.CategoryName == "Tư vấn sức khỏe");

            var services = new List<Service>
            {
                new Service { CategoryId = groomingCategory.Id, ServiceName = "Tắm và cắt tỉa cơ bản", Description = "Dịch vụ tắm, sấy và cắt tỉa lông cơ bản", DurationMinutes = 90, Price = 150000, IsActive = true },
                new Service { CategoryId = groomingCategory.Id, ServiceName = "Tắm và cắt tỉa cao cấp", Description = "Dịch vụ tắm, spa và cắt tỉa lông chuyên nghiệp", DurationMinutes = 120, Price = 300000, IsActive = true },
                new Service { CategoryId = spaCategory.Id, ServiceName = "Spa thư giãn", Description = "Massage, chăm sóc da lông cao cấp", DurationMinutes = 60, Price = 250000, IsActive = true },
                new Service { CategoryId = hotelCategory.Id, ServiceName = "Lưu trú thú cưng", Description = "Dịch vụ lưu trú theo ngày", DurationMinutes = 1440, Price = 200000, IsActive = true },
                new Service { CategoryId = consultationCategory.Id, ServiceName = "Tư vấn sức khỏe", Description = "Tư vấn và giới thiệu bác sĩ thú y uy tín", DurationMinutes = 30, Price = 0, IsActive = true }
            };
            await context.Services.AddRangeAsync(services);
            await context.SaveChangesAsync();

            // Seed Blog Categories
            var blogCategories = new List<BlogCategory>
            {
                new BlogCategory { CategoryName = "Chăm sóc sức khỏe", Slug = "cham-soc-suc-khoe", Description = "Bài viết về chăm sóc sức khỏe thú cưng" },
                new BlogCategory { CategoryName = "Dinh dưỡng", Slug = "dinh-duong", Description = "Bài viết về chế độ ăn uống" },
                new BlogCategory { CategoryName = "Huấn luyện", Slug = "huan-luyen", Description = "Bài viết về huấn luyện thú cưng" },
                new BlogCategory { CategoryName = "Câu chuyện", Slug = "cau-chuyen", Description = "Câu chuyện về thú cưng" },
                new BlogCategory { CategoryName = "Mẹo hay", Slug = "meo-hay", Description = "Các mẹo chăm sóc thú cưng" }
            };
            await context.BlogCategories.AddRangeAsync(blogCategories);
            await context.SaveChangesAsync();

            // Seed Product Categories
            var productCategories = new List<ProductCategory>
            {
                new ProductCategory { CategoryName = "Thức ăn", Description = "Thức ăn cho thú cưng", DisplayOrder = 1, IsActive = true },
                new ProductCategory { CategoryName = "Phụ kiện", Description = "Phụ kiện chăm sóc thú cưng", DisplayOrder = 2, IsActive = true },
                new ProductCategory { CategoryName = "Đồ chơi", Description = "Đồ chơi cho thú cưng", DisplayOrder = 3, IsActive = true },
                new ProductCategory { CategoryName = "Thuốc & Vitamin", Description = "Thuốc và vitamin bổ sung", DisplayOrder = 4, IsActive = true },
                new ProductCategory { CategoryName = "Vệ sinh", Description = "Sản phẩm vệ sinh", DisplayOrder = 5, IsActive = true },
                new ProductCategory { CategoryName = "Quần áo", Description = "Quần áo cho thú cưng", DisplayOrder = 6, IsActive = true }
            };
            await context.ProductCategories.AddRangeAsync(productCategories);
            await context.SaveChangesAsync();

            // Seed Brands
            var brands = new List<Brand>
            {
                new Brand { BrandName = "Royal Canin", Description = "Thương hiệu thức ăn cao cấp cho thú cưng" },
                new Brand { BrandName = "Pedigree", Description = "Thức ăn cho chó uy tín" },
                new Brand { BrandName = "Whiskas", Description = "Thức ăn cho mèo hàng đầu" },
                new Brand { BrandName = "Me-O", Description = "Thức ăn cho mèo giá tốt" },
                new Brand { BrandName = "SmartHeart", Description = "Thức ăn dinh dưỡng cho thú cưng" }
            };
            await context.Brands.AddRangeAsync(brands);
            await context.SaveChangesAsync();

            // Seed Branches
            var branches = new List<Branch>
            {
                new Branch 
                { 
                    BranchName = "PetCare - Chi nhánh Hà Nội",
                    Address = "123 Láng Hạ, Ba Đình, Hà Nội",
                    Phone = "024-1234-5678",
                    Email = "hanoi@petcare.com",
                    IsActive = true
                },
                new Branch 
                { 
                    BranchName = "PetCare - Chi nhánh TP.HCM",
                    Address = "456 Nguyễn Huệ, Quận 1, TP.HCM",
                    Phone = "028-8765-4321",
                    Email = "hcm@petcare.com",
                    IsActive = true
                }
            };
            await context.Branches.AddRangeAsync(branches);
            await context.SaveChangesAsync();

            // Seed Tags
            var tags = new List<Tag>
            {
                new Tag { TagName = "Sức khỏe", Slug = "suc-khoe" },
                new Tag { TagName = "Dinh dưỡng", Slug = "dinh-duong" },
                new Tag { TagName = "Huấn luyện", Slug = "huan-luyen" },
                new Tag { TagName = "Grooming", Slug = "grooming" },
                new Tag { TagName = "Mẹo hay", Slug = "meo-hay" }
            };
            await context.Tags.AddRangeAsync(tags);
            await context.SaveChangesAsync();

            // Seed FAQ Items
            var faqItems = new List<FaqItem>
            {
                new FaqItem 
                { 
                    Question = "PetCare cung cấp những dịch vụ gì?",
                    Answer = "PetCare là nền tảng kết nối cung cấp dịch vụ grooming, spa, khách sạn thú cưng, huấn luyện và tư vấn sức khỏe. Chúng tôi không cung cấp dịch vụ khám chữa bệnh trực tiếp nhưng có thể giới thiệu các phòng khám thú y uy tín.",
                    Category = "Dịch vụ",
                    Keywords = new[] { "dịch vụ", "grooming", "spa", "khách sạn" },
                    IsActive = true
                },
                new FaqItem 
                { 
                    Question = "Tôi nên cho thú cưng ăn gì?",
                    Answer = "Nên cho thú cưng ăn thức ăn chuyên dụng, cân đối dinh dưỡng theo độ tuổi và giống loài. PetCare có bán các sản phẩm thức ăn chất lượng cao từ các thương hiệu uy tín.",
                    Category = "Dinh dưỡng",
                    Keywords = new[] { "thức ăn", "dinh dưỡng", "chế độ ăn" },
                    IsActive = true
                }
            };
            await context.FaqItems.AddRangeAsync(faqItems);
            await context.SaveChangesAsync();

            Console.WriteLine("Database seeded successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding database: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Seed pet species and breeds from PetFinder API
    /// Maps Vietnamese species names to English PetFinder types
    /// </summary>
    public static async Task SeedFromPetFinderAsync(PetCareDbContext context, HttpClient httpClient, Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        try
        {
            Console.WriteLine("=== Fetching data from PetFinder API ===");
            
            // Check if breeds already exist
            if (await context.PetBreeds.AnyAsync())
            {
                Console.WriteLine("Pet breeds already exist. Skipping PetFinder import.");
                return;
            }

            // Get existing species from database (Vietnamese names)
            var existingSpecies = await context.PetSpecies.ToListAsync();
            
            if (!existingSpecies.Any())
            {
                Console.WriteLine("No species found. Please run normal seeder first.");
                return;
            }

            // Mapping Vietnamese to English species names for PetFinder API
            var speciesMapping = new Dictionary<string, string>
            {
                { "Chó", "Dog" },
                { "Mèo", "Cat" },
                { "Chim", "Bird" },
                { "Thỏ", "Rabbit" },
                { "Hamster", "Small & Furry" },
                { "Cá", "Scales, Fins & Other" }
            };

            var petFinderService = new PetFinderService(httpClient, configuration);
            var allBreeds = new List<PetBreed>();

            foreach (var species in existingSpecies)
            {
                if (speciesMapping.TryGetValue(species.SpeciesName, out var englishName))
                {
                    Console.WriteLine($"Fetching breeds for {species.SpeciesName} ({englishName})...");
                    
                    try
                    {
                        var breeds = await petFinderService.GetBreedsAsync(englishName, species.Id);
                        allBreeds.AddRange(breeds);
                        Console.WriteLine($"  ✓ Found {breeds.Count} breeds for {species.SpeciesName}");
                        
                        // Rate limiting - be nice to the API
                        await Task.Delay(500);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  ✗ Error fetching breeds for {species.SpeciesName}: {ex.Message}");
                    }
                }
            }

            if (allBreeds.Any())
            {
                await context.PetBreeds.AddRangeAsync(allBreeds);
                await context.SaveChangesAsync();
                Console.WriteLine($"✓ Imported {allBreeds.Count} breeds from PetFinder");
            }
            else
            {
                Console.WriteLine("⚠ No breeds were fetched. Using fallback data...");
                // Fallback to local data
                var fallbackBreeds = PetSpeciesSeedData.GetAllBreeds();
                
                // Update species IDs to match existing Vietnamese species
                foreach (var breed in fallbackBreeds)
                {
                    var vietnameseSpecies = existingSpecies.FirstOrDefault(s => 
                        (breed.SpeciesId.ToString().StartsWith("11111111") && s.SpeciesName == "Chó") ||
                        (breed.SpeciesId.ToString().StartsWith("22222222") && s.SpeciesName == "Mèo") ||
                        (breed.SpeciesId.ToString().StartsWith("33333333") && s.SpeciesName == "Chim") ||
                        (breed.SpeciesId.ToString().StartsWith("44444444") && s.SpeciesName == "Thỏ")
                    );
                    
                    if (vietnameseSpecies != null)
                    {
                        breed.SpeciesId = vietnameseSpecies.Id;
                    }
                }
                
                await context.PetBreeds.AddRangeAsync(fallbackBreeds);
                await context.SaveChangesAsync();
                Console.WriteLine($"✓ Used local seed data: {fallbackBreeds.Count} breeds");
            }

            Console.WriteLine("=== PetFinder import completed ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error importing from PetFinder: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}
