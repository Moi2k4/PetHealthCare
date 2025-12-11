using PetCare.Application.Common;
using PetCare.Application.DTOs.Category;

namespace PetCare.Application.Services.Interfaces;

public interface IProductCategoryService
{
    Task<ServiceResult<ProductCategoryDto>> GetCategoryByIdAsync(Guid categoryId);
    Task<ServiceResult<List<ProductCategoryDto>>> GetAllCategoriesAsync(bool includeInactive = false);
    Task<ServiceResult<List<ProductCategoryDto>>> GetCategoryHierarchyAsync();
    Task<ServiceResult<List<ProductCategoryDto>>> GetSubCategoriesAsync(Guid parentCategoryId);
    Task<ServiceResult<ProductCategoryDto>> CreateCategoryAsync(CreateProductCategoryDto dto);
    Task<ServiceResult<ProductCategoryDto>> UpdateCategoryAsync(Guid categoryId, UpdateProductCategoryDto dto);
    Task<ServiceResult<bool>> DeleteCategoryAsync(Guid categoryId);
    Task<ServiceResult<bool>> ToggleActiveStatusAsync(Guid categoryId);
}
