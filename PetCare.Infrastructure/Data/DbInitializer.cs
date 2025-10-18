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
                new Role { RoleName = "doctor", Description = "Bác sĩ thú y" },
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
                new ServiceCategory { CategoryName = "Khám bệnh", Description = "Dịch vụ khám và điều trị bệnh", IconUrl = "/icons/medical.svg" },
                new ServiceCategory { CategoryName = "Grooming", Description = "Dịch vụ cắt tỉa, tắm rửa", IconUrl = "/icons/grooming.svg" },
                new ServiceCategory { CategoryName = "Spa & Chăm sóc", Description = "Dịch vụ spa và chăm sóc sắc đẹp", IconUrl = "/icons/spa.svg" },
                new ServiceCategory { CategoryName = "Tiêm phòng", Description = "Dịch vụ tiêm phòng vắc-xin", IconUrl = "/icons/vaccine.svg" },
                new ServiceCategory { CategoryName = "Phẫu thuật", Description = "Dịch vụ phẫu thuật thú y", IconUrl = "/icons/surgery.svg" },
                new ServiceCategory { CategoryName = "Khách sạn thú cưng", Description = "Dịch vụ lưu trú thú cưng", IconUrl = "/icons/hotel.svg" }
            };
            await context.ServiceCategories.AddRangeAsync(serviceCategories);
            await context.SaveChangesAsync();

            // Seed Services
            var medicalCategory = serviceCategories.First(sc => sc.CategoryName == "Khám bệnh");
            var groomingCategory = serviceCategories.First(sc => sc.CategoryName == "Grooming");

            var services = new List<Service>
            {
                new Service { CategoryId = medicalCategory.Id, ServiceName = "Khám tổng quát", Description = "Khám sức khỏe tổng quát cho thú cưng", DurationMinutes = 30, Price = 200000, IsActive = true },
                new Service { CategoryId = medicalCategory.Id, ServiceName = "Khám chuyên sâu", Description = "Khám chuyên sâu với bác sĩ chuyên khoa", DurationMinutes = 60, Price = 500000, IsActive = true },
                new Service { CategoryId = groomingCategory.Id, ServiceName = "Tắm và cắt tỉa cơ bản", Description = "Dịch vụ tắm, sấy và cắt tỉa lông cơ bản", DurationMinutes = 90, Price = 150000, IsActive = true },
                new Service { CategoryId = groomingCategory.Id, ServiceName = "Tắm và cắt tỉa cao cấp", Description = "Dịch vụ tắm, spa và cắt tỉa lông chuyên nghiệp", DurationMinutes = 120, Price = 300000, IsActive = true }
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
                    Question = "Tôi nên tiêm phòng cho thú cưng khi nào?",
                    Answer = "Thú cưng nên được tiêm phòng đầy đủ từ 6-8 tuần tuổi. Các mũi tiêm tiếp theo theo lịch của bác sĩ thú y.",
                    Category = "Sức khỏe",
                    Keywords = new[] { "tiêm phòng", "vắc-xin", "sức khỏe" },
                    IsActive = true
                },
                new FaqItem 
                { 
                    Question = "Tôi nên cho thú cưng ăn gì?",
                    Answer = "Nên cho thú cưng ăn thức ăn chuyên dụng, cân đối dinh dưỡng theo độ tuổi và giống loài.",
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
