using AutoMapper;
using PetCare.Application.DTOs.User;
using PetCare.Application.DTOs.Pet;
using PetCare.Application.DTOs.Product;
using PetCare.Application.DTOs.Order;
using PetCare.Application.DTOs.Appointment;
using PetCare.Application.DTOs.Blog;
using PetCare.Application.DTOs.Category;
using PetCare.Application.DTOs.Service;
using PetCare.Application.DTOs.Subscription;
using PetCare.Application.DTOs.Notification;
using PetCare.Application.DTOs.Review;
using PetCare.Application.DTOs.Health;
using PetCare.Application.DTOs.Payment;
using PetCare.Application.DTOs.Voucher;
using PetCare.Application.DTOs.Chat;
using PetCare.Domain.Entities;

namespace PetCare.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : null));
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Pet mappings
        CreateMap<Pet, PetDto>()
            .ForMember(dest => dest.SpeciesName, opt => opt.MapFrom(src => src.Species != null ? src.Species.SpeciesName : null))
            .ForMember(dest => dest.BreedName, opt => opt.MapFrom(src => src.Breed != null ? src.Breed.BreedName : null));
        CreateMap<CreatePetDto, Pet>();
        CreateMap<UpdatePetDto, Pet>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Pet Species and Breed mappings
        CreateMap<PetSpecies, PetSpeciesDto>();
        CreateMap<PetBreed, PetBreedDto>()
            .ForMember(dest => dest.SpeciesName, opt => opt.MapFrom(src => src.Species != null ? src.Species.SpeciesName : null));
        CreateMap<PetSpecies, SpeciesWithBreedsDto>()
            .ForMember(dest => dest.Breeds, opt => opt.MapFrom(src => src.Breeds));

        // Product mappings
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList()));
             
        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.Images, opt => opt.Ignore()); // Handled manually service side
        CreateMap<UpdateProductDto, Product>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Order mappings
        CreateMap<Order, OrderDto>();
        CreateMap<OrderItem, OrderItemDto>();

        // Appointment mappings
        CreateMap<Appointment, AppointmentDto>()
            .ForMember(dest => dest.PetName, opt => opt.MapFrom(src => src.Pet != null ? src.Pet.PetName : null))
            .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service != null ? src.Service.ServiceName : null))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.BranchName : null))
            .ForMember(dest => dest.StaffName, opt => opt.MapFrom(src => src.AssignedStaff != null ? src.AssignedStaff.FullName : null));

        // Blog mappings
        CreateMap<BlogPost, BlogPostDto>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.FullName : null))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.BlogPostTags.Select(pt => pt.Tag.TagName).ToList()));

        // Product Category mappings
        CreateMap<ProductCategory, ProductCategoryDto>();

        // Service mappings
        CreateMap<Service, ServiceDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null));
        CreateMap<ServiceCategory, ServiceCategoryDto>()
            .ForMember(dest => dest.ServiceCount, opt => opt.MapFrom(src => src.Services.Count));

        // Subscription mappings
        CreateMap<SubscriptionPackage, SubscriptionPackageDto>();
        CreateMap<UserSubscription, UserSubscriptionDto>()
            .ForMember(dest => dest.PackageName, opt => opt.MapFrom(src => src.SubscriptionPackage.Name));

        // Notification mappings
        CreateMap<Notification, NotificationDto>();

        // Review mappings
        CreateMap<ProductReview, ProductReviewDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null));
        CreateMap<ServiceReview, ServiceReviewDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null))
            .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service != null ? src.Service.ServiceName : null));

        // Health tracking mappings
        CreateMap<HealthRecord, HealthRecordDto>()
            .ForMember(dest => dest.PetName, opt => opt.MapFrom(src => src.Pet != null ? src.Pet.PetName : null));
        CreateMap<Vaccination, VaccinationDto>()
            .ForMember(dest => dest.PetName, opt => opt.MapFrom(src => src.Pet != null ? src.Pet.PetName : null))
            .ForMember(dest => dest.IsOverdue, opt => opt.MapFrom(src => src.NextDueDate.HasValue && src.NextDueDate.Value < DateTime.UtcNow));
        CreateMap<HealthReminder, HealthReminderDto>()
            .ForMember(dest => dest.PetName, opt => opt.MapFrom(src => src.Pet != null ? src.Pet.PetName : null))
            .ForMember(dest => dest.IsUpcoming, opt => opt.MapFrom(src => !src.IsCompleted && src.ReminderDate >= DateTime.UtcNow && src.ReminderDate <= DateTime.UtcNow.AddDays(7)));

        // Payment mappings
        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderNumber : string.Empty));

        // Voucher mappings
        CreateMap<Voucher, VoucherDto>();
        CreateMap<VoucherUsage, VoucherDto>();

        // Chat mappings
        CreateMap<ChatSession, ChatSessionDto>();
        CreateMap<ChatMessage, ChatMessageDto>();
    }
}
