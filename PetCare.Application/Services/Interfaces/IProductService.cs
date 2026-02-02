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
    
    // Management methods
    Task<ServiceResult<ProductDto>> CreateProductAsync(CreateProductDto createProductDto);
    Task<ServiceResult<ProductDto>> UpdateProductAsync(Guid productId, UpdateProductDto updateProductDto);
    Task<ServiceResult<bool>> DeleteProductAsync(Guid productId);
    Task<ServiceResult<ProductDto>> UpdateStockAsync(Guid productId, int quantity);
    
    // Image management methods
    Task<ServiceResult<ProductDto>> AddProductImageAsync(Guid productId, string imageUrl);
    Task<ServiceResult<bool>> RemoveProductImageAsync(Guid productId, Guid imageId);
}
