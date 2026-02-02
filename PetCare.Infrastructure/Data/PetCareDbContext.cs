using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Common;
using PetCare.Domain.Entities;

namespace PetCare.Infrastructure.Data;

public class PetCareDbContext : DbContext
{
    public PetCareDbContext(DbContextOptions<PetCareDbContext> options) : base(options)
    {
    }

    // User Management
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }

    // Pet Management
    public DbSet<PetSpecies> PetSpecies { get; set; }
    public DbSet<PetBreed> PetBreeds { get; set; }
    public DbSet<Pet> Pets { get; set; }
    public DbSet<HealthRecord> HealthRecords { get; set; }
    public DbSet<Vaccination> Vaccinations { get; set; }
    public DbSet<HealthReminder> HealthReminders { get; set; }

    // E-Commerce
    public DbSet<ProductCategory> ProductCategories { get; set; }

    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

    // Services & Appointments
    public DbSet<Branch> Branches { get; set; }
    public DbSet<ServiceCategory> ServiceCategories { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<StaffService> StaffServices { get; set; }
    public DbSet<StaffSchedule> StaffSchedules { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<AppointmentStatusHistory> AppointmentStatusHistories { get; set; }

    // Blog & Community
    public DbSet<BlogCategory> BlogCategories { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<BlogPostTag> BlogPostTags { get; set; }
    public DbSet<BlogComment> BlogComments { get; set; }
    public DbSet<BlogLike> BlogLikes { get; set; }

    // Chat & Support
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<FaqItem> FaqItems { get; set; }

    // Reviews & Notifications
    public DbSet<ProductReview> ProductReviews { get; set; }
    public DbSet<ServiceReview> ServiceReviews { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    // Subscriptions & AI
    public DbSet<SubscriptionPackage> SubscriptionPackages { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }
    public DbSet<AIHealthAnalysis> AIHealthAnalyses { get; set; }

    // Payment & Vouchers
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Voucher> Vouchers { get; set; }
    public DbSet<VoucherUsage> VoucherUsages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema
        modelBuilder.HasDefaultSchema("petcare");

        // Configure entities
        ConfigureUserEntities(modelBuilder);
        ConfigurePetEntities(modelBuilder);
        ConfigureProductEntities(modelBuilder);
        ConfigureServiceEntities(modelBuilder);
        ConfigureBlogEntities(modelBuilder);
        ConfigureChatEntities(modelBuilder);
        ConfigureReviewEntities(modelBuilder);
        ConfigureSubscriptionEntities(modelBuilder);
        ConfigurePaymentEntities(modelBuilder);
        ConfigureVoucherEntities(modelBuilder);
    }

    private void ConfigureUserEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.FullName).HasColumnName("full_name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.City).HasColumnName("city").HasMaxLength(100);
            entity.Property(e => e.District).HasColumnName("district").HasMaxLength(100);
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();

            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RoleName).HasColumnName("role_name").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.RoleName).IsUnique();
        });
    }

    private void ConfigurePetEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PetSpecies>(entity =>
        {
            entity.ToTable("pet_species");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SpeciesName).HasColumnName("species_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<PetBreed>(entity =>
        {
            entity.ToTable("pet_breeds");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SpeciesId).HasColumnName("species_id");
            entity.Property(e => e.BreedName).HasColumnName("breed_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Characteristics).HasColumnName("characteristics");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Species)
                .WithMany(s => s.Breeds)
                .HasForeignKey(e => e.SpeciesId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.ToTable("pets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.SpeciesId).HasColumnName("species_id");
            entity.Property(e => e.BreedId).HasColumnName("breed_id");
            entity.Property(e => e.PetName).HasColumnName("pet_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Gender).HasColumnName("gender").HasMaxLength(10);
            entity.Property(e => e.Weight).HasColumnName("weight").HasPrecision(5, 2);
            entity.Property(e => e.Color).HasColumnName("color").HasMaxLength(100);
            entity.Property(e => e.MicrochipId).HasColumnName("microchip_id").HasMaxLength(50);
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.SpecialNotes).HasColumnName("special_notes");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Pets)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Species)
                .WithMany(s => s.Pets)
                .HasForeignKey(e => e.SpeciesId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Breed)
                .WithMany(b => b.Pets)
                .HasForeignKey(e => e.BreedId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.UserId);
        });

        modelBuilder.Entity<HealthRecord>(entity =>
        {
            entity.ToTable("health_records");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PetId).HasColumnName("pet_id");
            entity.Property(e => e.RecordDate).HasColumnName("record_date");
            entity.Property(e => e.Weight).HasColumnName("weight").HasPrecision(5, 2);
            entity.Property(e => e.Height).HasColumnName("height").HasPrecision(5, 2);
            entity.Property(e => e.Temperature).HasColumnName("temperature").HasPrecision(4, 2);
            entity.Property(e => e.HeartRate).HasColumnName("heart_rate");
            entity.Property(e => e.Diagnosis).HasColumnName("diagnosis");
            entity.Property(e => e.Treatment).HasColumnName("treatment");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.RecordedBy).HasColumnName("recorded_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Pet)
                .WithMany(p => p.HealthRecords)
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RecordedByUser)
                .WithMany()
                .HasForeignKey(e => e.RecordedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.PetId);
        });

        modelBuilder.Entity<Vaccination>(entity =>
        {
            entity.ToTable("vaccinations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PetId).HasColumnName("pet_id");
            entity.Property(e => e.VaccineName).HasColumnName("vaccine_name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.VaccinationDate).HasColumnName("vaccination_date");
            entity.Property(e => e.NextDueDate).HasColumnName("next_due_date");
            entity.Property(e => e.BatchNumber).HasColumnName("batch_number").HasMaxLength(100);
            entity.Property(e => e.AdministeredBy).HasColumnName("administered_by");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Pet)
                .WithMany(p => p.Vaccinations)
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AdministeredByUser)
                .WithMany()
                .HasForeignKey(e => e.AdministeredBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.PetId);
        });

        modelBuilder.Entity<HealthReminder>(entity =>
        {
            entity.ToTable("health_reminders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PetId).HasColumnName("pet_id");
            entity.Property(e => e.ReminderType).HasColumnName("reminder_type").IsRequired().HasMaxLength(50);
            entity.Property(e => e.ReminderTitle).HasColumnName("reminder_title").IsRequired().HasMaxLength(255);
            entity.Property(e => e.ReminderDate).HasColumnName("reminder_date");
            entity.Property(e => e.IsCompleted).HasColumnName("is_completed");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Pet)
                .WithMany(p => p.HealthReminders)
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.PetId);
        });
    }

    private void ConfigureProductEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.ToTable("product_categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryName).HasColumnName("category_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.ParentCategoryId).HasColumnName("parent_category_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });



        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");

            entity.Property(e => e.ProductName).HasColumnName("product_name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Price).HasColumnName("price").HasPrecision(12, 2);
            entity.Property(e => e.SalePrice).HasColumnName("sale_price").HasPrecision(12, 2);
            entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity");
            entity.Property(e => e.Sku).HasColumnName("sku").HasMaxLength(100);
            entity.Property(e => e.Weight).HasColumnName("weight").HasPrecision(8, 2);
            entity.Property(e => e.Dimensions).HasColumnName("dimensions").HasMaxLength(50);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);



            entity.HasIndex(e => e.Sku).IsUnique();
            entity.HasIndex(e => e.CategoryId);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("product_images");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url").IsRequired();
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.IsPrimary).HasColumnName("is_primary");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ProductId);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.ToTable("cart_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.User)
                .WithMany(u => u.CartItems)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.OrderNumber).HasColumnName("order_number").IsRequired().HasMaxLength(50);
            entity.Property(e => e.OrderStatus).HasColumnName("order_status").IsRequired().HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasPrecision(12, 2);
            entity.Property(e => e.ShippingFee).HasColumnName("shipping_fee").HasPrecision(10, 2);
            entity.Property(e => e.DiscountAmount).HasColumnName("discount_amount").HasPrecision(10, 2);
            entity.Property(e => e.FinalAmount).HasColumnName("final_amount").HasPrecision(12, 2);
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50);
            entity.Property(e => e.PaymentStatus).HasColumnName("payment_status").HasMaxLength(50);
            entity.Property(e => e.ShippingAddress).HasColumnName("shipping_address").IsRequired();
            entity.Property(e => e.ShippingPhone).HasColumnName("shipping_phone").IsRequired().HasMaxLength(20);
            entity.Property(e => e.ShippingName).HasColumnName("shipping_name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.OrderedAt).HasColumnName("ordered_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => e.UserId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductName).HasColumnName("product_name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UnitPrice).HasColumnName("unit_price").HasPrecision(12, 2);
            entity.Property(e => e.TotalPrice).HasColumnName("total_price").HasPrecision(12, 2);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.OrderId);
        });

        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.ToTable("order_status_history");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Order)
                .WithMany(o => o.StatusHistory)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UpdatedByUser)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.OrderId);
        });
    }

    private void ConfigureServiceEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.ToTable("branches");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BranchName).HasColumnName("branch_name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Address).HasColumnName("address").IsRequired();
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(e => e.OpeningHours).HasColumnName("opening_hours");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.ToTable("service_categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryName).HasColumnName("category_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IconUrl).HasColumnName("icon_url");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.ToTable("services");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.ServiceName).HasColumnName("service_name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.Price).HasColumnName("price").HasPrecision(10, 2);
            entity.Property(e => e.IsHomeService).HasColumnName("is_home_service");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Services)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StaffService>(entity =>
        {
            entity.ToTable("staff_services");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Service)
                .WithMany(s => s.StaffServices)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.ServiceId }).IsUnique();
        });

        modelBuilder.Entity<StaffSchedule>(entity =>
        {
            entity.ToTable("staff_schedules");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.WorkDate).HasColumnName("work_date");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.IsAvailable).HasColumnName("is_available");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Branch)
                .WithMany(b => b.StaffSchedules)
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("appointments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.PetId).HasColumnName("pet_id");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.AppointmentType).HasColumnName("appointment_type").IsRequired().HasMaxLength(50);
            entity.Property(e => e.AppointmentStatus).HasColumnName("appointment_status").HasMaxLength(50);
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.AssignedStaffId).HasColumnName("assigned_staff_id");
            entity.Property(e => e.AppointmentDate).HasColumnName("appointment_date");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.ServiceAddress).HasColumnName("service_address");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CancellationReason).HasColumnName("cancellation_reason");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Appointments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Pet)
                .WithMany(p => p.Appointments)
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Service)
                .WithMany(s => s.Appointments)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Branch)
                .WithMany(b => b.Appointments)
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignedStaff)
                .WithMany()
                .HasForeignKey(e => e.AssignedStaffId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.AppointmentDate);
        });

        modelBuilder.Entity<AppointmentStatusHistory>(entity =>
        {
            entity.ToTable("appointment_status_history");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Appointment)
                .WithMany(a => a.StatusHistory)
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UpdatedByUser)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.AppointmentId);
        });
    }

    private void ConfigureBlogEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogCategory>(entity =>
        {
            entity.ToTable("blog_categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryName).HasColumnName("category_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Slug).HasColumnName("slug").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.Slug).IsUnique();
        });

        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.ToTable("blog_posts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Title).HasColumnName("title").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Slug).HasColumnName("slug").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Content).HasColumnName("content").IsRequired();
            entity.Property(e => e.FeaturedImageUrl).HasColumnName("featured_image_url");
            entity.Property(e => e.Excerpt).HasColumnName("excerpt");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(e => e.ViewCount).HasColumnName("view_count");
            entity.Property(e => e.PublishedAt).HasColumnName("published_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Author)
                .WithMany(u => u.BlogPosts)
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.BlogPosts)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TagName).HasColumnName("tag_name").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Slug).HasColumnName("slug").IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.TagName).IsUnique();
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        modelBuilder.Entity<BlogPostTag>(entity =>
        {
            entity.ToTable("blog_post_tags");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.TagId).HasColumnName("tag_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Post)
                .WithMany(p => p.BlogPostTags)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                .WithMany(t => t.BlogPostTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.PostId, e.TagId }).IsUnique();
        });

        modelBuilder.Entity<BlogComment>(entity =>
        {
            entity.ToTable("blog_comments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ParentCommentId).HasColumnName("parent_comment_id");
            entity.Property(e => e.CommentText).HasColumnName("comment_text").IsRequired();
            entity.Property(e => e.IsApproved).HasColumnName("is_approved");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(e => e.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.PostId);
        });

        modelBuilder.Entity<BlogLike>(entity =>
        {
            entity.ToTable("blog_likes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.PostId, e.UserId }).IsUnique();
        });
    }

    private void ConfigureChatEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.ToTable("chat_sessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.SessionStart).HasColumnName("session_start");
            entity.Property(e => e.SessionEnd).HasColumnName("session_end");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.User)
                .WithMany(u => u.ChatSessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.UserId);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("chat_messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.SenderType).HasColumnName("sender_type").IsRequired().HasMaxLength(20);
            entity.Property(e => e.MessageText).HasColumnName("message_text").IsRequired();
            entity.Property(e => e.MessageMetadata).HasColumnName("message_metadata");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Session)
                .WithMany(s => s.Messages)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SessionId);
        });

        modelBuilder.Entity<FaqItem>(entity =>
        {
            entity.ToTable("faq_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Question).HasColumnName("question").IsRequired();
            entity.Property(e => e.Answer).HasColumnName("answer").IsRequired();
            entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(100);
            entity.Property(e => e.Keywords).HasColumnName("keywords");
            entity.Property(e => e.UsageCount).HasColumnName("usage_count");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });
    }

    private void ConfigureReviewEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.ToTable("product_reviews");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.ReviewText).HasColumnName("review_text");
            entity.Property(e => e.Images).HasColumnName("images");
            entity.Property(e => e.IsVerifiedPurchase).HasColumnName("is_verified_purchase");
            entity.Property(e => e.IsApproved).HasColumnName("is_approved");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Order)
                .WithMany(o => o.Reviews)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.ProductId);
        });

        modelBuilder.Entity<ServiceReview>(entity =>
        {
            entity.ToTable("service_reviews");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.ReviewText).HasColumnName("review_text");
            entity.Property(e => e.IsApproved).HasColumnName("is_approved");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Appointment)
                .WithMany(a => a.Reviews)
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Service)
                .WithMany(s => s.Reviews)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Staff)
                .WithMany()
                .HasForeignKey(e => e.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.AppointmentId);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.NotificationType).HasColumnName("notification_type").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).HasColumnName("title").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Message).HasColumnName("message").IsRequired();
            entity.Property(e => e.LinkUrl).HasColumnName("link_url");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.IsRead }).HasFilter("is_read = false");
        });
    }

    private void ConfigureSubscriptionEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionPackage>(entity =>
        {
            entity.ToTable("subscription_packages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description").IsRequired();
            entity.Property(e => e.Price).HasColumnName("price").HasPrecision(10, 2);
            entity.Property(e => e.BillingCycle).HasColumnName("billing_cycle").IsRequired().HasMaxLength(20);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.HasAIHealthTracking).HasColumnName("has_ai_health_tracking");
            entity.Property(e => e.HasVaccinationTracking).HasColumnName("has_vaccination_tracking");
            entity.Property(e => e.HasHealthReminders).HasColumnName("has_health_reminders");
            entity.Property(e => e.HasAIRecommendations).HasColumnName("has_ai_recommendations");
            entity.Property(e => e.HasNutritionalAnalysis).HasColumnName("has_nutritional_analysis");
            entity.Property(e => e.HasEarlyDiseaseDetection).HasColumnName("has_early_disease_detection");
            entity.Property(e => e.HasPrioritySupport).HasColumnName("has_priority_support");
            entity.Property(e => e.MaxPets).HasColumnName("max_pets");
            entity.Property(e => e.Features).HasColumnName("features");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.ToTable("user_subscriptions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.SubscriptionPackageId).HasColumnName("subscription_package_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(20);
            entity.Property(e => e.NextBillingDate).HasColumnName("next_billing_date");
            entity.Property(e => e.AmountPaid).HasColumnName("amount_paid").HasPrecision(10, 2);
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50);
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id").HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserSubscriptions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SubscriptionPackage)
                .WithMany(sp => sp.UserSubscriptions)
                .HasForeignKey(e => e.SubscriptionPackageId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.IsActive });
        });

        modelBuilder.Entity<AIHealthAnalysis>(entity =>
        {
            entity.ToTable("ai_health_analyses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PetId).HasColumnName("pet_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AnalysisType).HasColumnName("analysis_type").IsRequired().HasMaxLength(50);
            entity.Property(e => e.InputData).HasColumnName("input_data").IsRequired();
            entity.Property(e => e.AIResponse).HasColumnName("ai_response").IsRequired();
            entity.Property(e => e.Recommendations).HasColumnName("recommendations");
            entity.Property(e => e.ConfidenceScore).HasColumnName("confidence_score").HasPrecision(5, 2);
            entity.Property(e => e.TokensUsed).HasColumnName("tokens_used");
            entity.Property(e => e.AIModel).HasColumnName("ai_model").HasMaxLength(50);
            entity.Property(e => e.IsReviewed).HasColumnName("is_reviewed");
            entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.ReviewNotes).HasColumnName("review_notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Pet)
                .WithMany(p => p.AIHealthAnalyses)
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.AIHealthAnalyses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Reviewer)
                .WithMany()
                .HasForeignKey(e => e.ReviewedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.PetId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.PetId, e.AnalysisType });
        });
    }

    private void ConfigurePaymentEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method").IsRequired().HasMaxLength(50);
            entity.Property(e => e.PaymentStatus).HasColumnName("payment_status").IsRequired().HasMaxLength(20);
            entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(10, 2);
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id").HasMaxLength(255);
            entity.Property(e => e.PaymentGatewayResponse).HasColumnName("payment_gateway_response");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.RefundReason).HasColumnName("refund_reason");
            entity.Property(e => e.RefundedAt).HasColumnName("refunded_at");
            entity.Property(e => e.RefundAmount).HasColumnName("refund_amount").HasPrecision(10, 2);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.TransactionId);
        });
    }

    private void ConfigureVoucherEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.ToTable("vouchers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code).HasColumnName("code").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DiscountType).HasColumnName("discount_type").IsRequired().HasMaxLength(20);
            entity.Property(e => e.DiscountValue).HasColumnName("discount_value").HasPrecision(10, 2);
            entity.Property(e => e.MinimumOrderAmount).HasColumnName("minimum_order_amount").HasPrecision(10, 2);
            entity.Property(e => e.MaximumDiscountAmount).HasColumnName("maximum_discount_amount").HasPrecision(10, 2);
            entity.Property(e => e.UsageLimit).HasColumnName("usage_limit");
            entity.Property(e => e.UsedCount).HasColumnName("used_count");
            entity.Property(e => e.ValidFrom).HasColumnName("valid_from");
            entity.Property(e => e.ValidTo).HasColumnName("valid_to");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ApplicableProductCategories).HasColumnName("applicable_product_categories");
            entity.Property(e => e.ApplicableServices).HasColumnName("applicable_services");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => new { e.IsActive, e.ValidFrom, e.ValidTo });
        });

        modelBuilder.Entity<VoucherUsage>(entity =>
        {
            entity.ToTable("voucher_usages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.DiscountAmount).HasColumnName("discount_amount").HasPrecision(10, 2);
            entity.Property(e => e.UsedAt).HasColumnName("used_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Voucher)
                .WithMany(v => v.VoucherUsages)
                .HasForeignKey(e => e.VoucherId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.VoucherId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.OrderId);
        });
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is AuditableEntity && (e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            ((AuditableEntity)entry.Entity).UpdatedAt = DateTime.UtcNow;
        }
    }
}
