using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Entities;

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

            // Seed Pet Species
            var species = new List<PetSpecies>
            {
                new PetSpecies { SpeciesName = "Chó", Description = "Chó cảnh và chó làm việc" },
                new PetSpecies { SpeciesName = "Mèo", Description = "Mèo cảnh các loại" },
                new PetSpecies { SpeciesName = "Thỏ", Description = "Thỏ cảnh" },
                new PetSpecies { SpeciesName = "Hamster", Description = "Chuột Hamster" },
                new PetSpecies { SpeciesName = "Chim", Description = "Chim cảnh các loại" },
                new PetSpecies { SpeciesName = "Cá", Description = "Cá cảnh" }
            };
            await context.PetSpecies.AddRangeAsync(species);
            await context.SaveChangesAsync();

            // Seed Pet Breeds (for Dogs and Cats)
            var dogSpecies = species.First(s => s.SpeciesName == "Chó");
            var catSpecies = species.First(s => s.SpeciesName == "Mèo");

            var breeds = new List<PetBreed>
            {
                // Dog Breeds
                new PetBreed { SpeciesId = dogSpecies.Id, BreedName = "Golden Retriever", Characteristics = "Thân thiện, thông minh" },
                new PetBreed { SpeciesId = dogSpecies.Id, BreedName = "Poodle", Characteristics = "Thông minh, dễ huấn luyện" },
                new PetBreed { SpeciesId = dogSpecies.Id, BreedName = "Corgi", Characteristics = "Năng động, trung thành" },
                new PetBreed { SpeciesId = dogSpecies.Id, BreedName = "Shiba Inu", Characteristics = "Độc lập, trung thành" },
                
                // Cat Breeds
                new PetBreed { SpeciesId = catSpecies.Id, BreedName = "British Shorthair", Characteristics = "Điềm tĩnh, dễ gần" },
                new PetBreed { SpeciesId = catSpecies.Id, BreedName = "Scottish Fold", Characteristics = "Hiền lành, yêu cầu vuốt ve" },
                new PetBreed { SpeciesId = catSpecies.Id, BreedName = "Persian", Characteristics = "Lông dài, cần chăm sóc nhiều" },
                new PetBreed { SpeciesId = catSpecies.Id, BreedName = "Munchkin", Characteristics = "Chân ngắn, năng động" }
            };
            await context.PetBreeds.AddRangeAsync(breeds);
            await context.SaveChangesAsync();

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
}
