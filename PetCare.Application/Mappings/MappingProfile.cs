using AutoMapper;
using PetCare.Application.DTOs.User;
using PetCare.Application.DTOs.Pet;
using PetCare.Application.DTOs.Product;
using PetCare.Application.DTOs.Order;
using PetCare.Application.DTOs.Appointment;
using PetCare.Application.DTOs.Blog;
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
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.BrandName : null))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList()));

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
    }
}
