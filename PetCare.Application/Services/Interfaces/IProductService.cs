using PetCare.Application.DTOs.Product;
using PetCare.Application.Common;

namespace PetCare.Application.Services.Interfaces;

public interface IProductService
{
    Task<ServiceResult<ProductDto>> GetProductByIdAsync(Guid productId);
    Task<ServiceResult<PagedResult<ProductDto>>> GetProductsAsync(int page, int pageSize);
    Task<ServiceResult<IEnumerable<ProductDto>>> GetProductsByCategoryAsync(Guid categoryId);
    Task<ServiceResult<IEnumerable<ProductDto>>> SearchProductsAsync(string searchTerm);
    Task<ServiceResult<IEnumerable<ProductDto>>> GetActiveProductsAsync();
}
