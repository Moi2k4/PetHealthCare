using PetCare.Domain.Entities;

namespace PetCare.Infrastructure.Repositories.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(Guid categoryId);
    Task<Product?> GetProductWithImagesAsync(Guid productId);
    Task<IEnumerable<Product>> GetActiveProductsAsync();
    Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
    Task<Product?> GetProductBySkuAsync(string sku);
}
