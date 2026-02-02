using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Data;
using PetCare.Infrastructure.Repositories.Interfaces;

namespace PetCare.Infrastructure.Repositories.Implementations;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(PetCareDbContext context) : base(context)
    {
    }

    public async Task DeleteProductImagesAsync(Guid productId)
    {
        var images = await _context.ProductImages
            .Where(p => p.ProductId == productId)
            .ToListAsync();
        
        _context.ProductImages.RemoveRange(images);
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(Guid categoryId)
    {
        return await _dbSet
            .Include(p => p.Category)

            .Include(p => p.Images)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .ToListAsync();
    }

    public async Task<Product?> GetProductWithImagesAsync(Guid productId)
    {
        return await _dbSet
            .Include(p => p.Category)

            .Include(p => p.Images)
            .Include(p => p.Reviews.Where(r => r.IsApproved))
            .FirstOrDefaultAsync(p => p.Id == productId);
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        return await _dbSet
            .Include(p => p.Category)

            .Include(p => p.Images)
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
    {
        return await _dbSet
            .Include(p => p.Category)

            .Include(p => p.Images)
            .Where(p => p.IsActive && 
                (p.ProductName.Contains(searchTerm) || 
                 (p.Description != null && p.Description.Contains(searchTerm))))
            .ToListAsync();
    }

    public async Task<Product?> GetProductBySkuAsync(string sku)
    {
        return await _dbSet
            .Include(p => p.Category)

            .FirstOrDefaultAsync(p => p.Sku == sku);
    }
}
