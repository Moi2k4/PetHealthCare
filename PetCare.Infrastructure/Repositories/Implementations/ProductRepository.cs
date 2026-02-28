using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Data;
using PetCare.Infrastructure.Repositories.Interfaces;
using System.Globalization;
using System.Text;

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
        var normalizedSearch = RemoveDiacritics(searchTerm);

        // Load active products into memory first so the C# normalization can run
        var activeProducts = await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.IsActive)
            .ToListAsync();

        return activeProducts.Where(p =>
            RemoveDiacritics(p.ProductName).Contains(normalizedSearch) ||
            (p.Description != null && RemoveDiacritics(p.Description).Contains(normalizedSearch)));
    }

    private static string RemoveDiacritics(string text)
    {
        // Handle đ/Đ which does not decompose via FormD
        text = text.Replace('Đ', 'D').Replace('đ', 'd');

        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).ToLower();
    }

    public async Task<Product?> GetProductBySkuAsync(string sku)
    {
        return await _dbSet
            .Include(p => p.Category)

            .FirstOrDefaultAsync(p => p.Sku == sku);
    }
}
