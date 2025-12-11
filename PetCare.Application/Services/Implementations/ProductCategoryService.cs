using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetCare.Application.Common;
using PetCare.Application.DTOs.Category;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Data;

namespace PetCare.Application.Services.Implementations;

public class ProductCategoryService : IProductCategoryService
{
    private readonly PetCareDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductCategoryService> _logger;

    public ProductCategoryService(
        PetCareDbContext context,
        IMapper mapper,
        ILogger<ProductCategoryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ServiceResult<ProductCategoryDto>> GetCategoryByIdAsync(Guid categoryId)
    {
        try
        {
            var category = await _context.ProductCategories
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
                return ServiceResult<ProductCategoryDto>.FailureResult("Category not found");

            var dto = _mapper.Map<ProductCategoryDto>(category);
            dto.ProductCount = category.Products.Count;

            return ServiceResult<ProductCategoryDto>.SuccessResult(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category by ID: {CategoryId}", categoryId);
            return ServiceResult<ProductCategoryDto>.FailureResult($"Error retrieving category: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<ProductCategoryDto>>> GetAllCategoriesAsync(bool includeInactive = false)
    {
        try
        {
            var query = _context.ProductCategories
                .Include(c => c.Products)
                .AsQueryable();

            if (!includeInactive)
                query = query.Where(c => c.IsActive);

            var categories = await query
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();

            var dtos = categories.Select(c =>
            {
                var dto = _mapper.Map<ProductCategoryDto>(c);
                dto.ProductCount = c.Products.Count;
                return dto;
            }).ToList();

            return ServiceResult<List<ProductCategoryDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all categories");
            return ServiceResult<List<ProductCategoryDto>>.FailureResult($"Error retrieving categories: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<ProductCategoryDto>>> GetCategoryHierarchyAsync()
    {
        try
        {
            var allCategories = await _context.ProductCategories
                .Include(c => c.Products)
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var rootCategories = allCategories.Where(c => c.ParentCategoryId == null).ToList();
            var dtos = BuildCategoryTree(rootCategories, allCategories);

            return ServiceResult<List<ProductCategoryDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category hierarchy");
            return ServiceResult<List<ProductCategoryDto>>.FailureResult($"Error retrieving category hierarchy: {ex.Message}");
        }
    }

    private List<ProductCategoryDto> BuildCategoryTree(List<ProductCategory> rootCategories, List<ProductCategory> allCategories)
    {
        var dtos = new List<ProductCategoryDto>();

        foreach (var category in rootCategories)
        {
            var dto = _mapper.Map<ProductCategoryDto>(category);
            dto.ProductCount = category.Products.Count;

            var children = allCategories.Where(c => c.ParentCategoryId == category.Id).ToList();
            if (children.Any())
            {
                dto.SubCategories = BuildCategoryTree(children, allCategories);
            }

            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<ServiceResult<List<ProductCategoryDto>>> GetSubCategoriesAsync(Guid parentCategoryId)
    {
        try
        {
            var subCategories = await _context.ProductCategories
                .Include(c => c.Products)
                .Where(c => c.ParentCategoryId == parentCategoryId && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var dtos = subCategories.Select(c =>
            {
                var dto = _mapper.Map<ProductCategoryDto>(c);
                dto.ProductCount = c.Products.Count;
                return dto;
            }).ToList();

            return ServiceResult<List<ProductCategoryDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subcategories for parent: {ParentCategoryId}", parentCategoryId);
            return ServiceResult<List<ProductCategoryDto>>.FailureResult($"Error retrieving subcategories: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProductCategoryDto>> CreateCategoryAsync(CreateProductCategoryDto dto)
    {
        try
        {
            if (dto.ParentCategoryId.HasValue)
            {
                var parentExists = await _context.ProductCategories
                    .AnyAsync(c => c.Id == dto.ParentCategoryId.Value);
                
                if (!parentExists)
                    return ServiceResult<ProductCategoryDto>.FailureResult("Parent category not found");
            }

            var category = new ProductCategory
            {
                CategoryName = dto.CategoryName,
                ParentCategoryId = dto.ParentCategoryId,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                DisplayOrder = dto.DisplayOrder,
                IsActive = true
            };

            _context.ProductCategories.Add(category);
            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<ProductCategoryDto>(category);
            resultDto.ProductCount = 0;

            return ServiceResult<ProductCategoryDto>.SuccessResult(resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return ServiceResult<ProductCategoryDto>.FailureResult($"Error creating category: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProductCategoryDto>> UpdateCategoryAsync(Guid categoryId, UpdateProductCategoryDto dto)
    {
        try
        {
            var category = await _context.ProductCategories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
                return ServiceResult<ProductCategoryDto>.FailureResult("Category not found");

            if (dto.ParentCategoryId.HasValue)
            {
                if (dto.ParentCategoryId.Value == categoryId)
                    return ServiceResult<ProductCategoryDto>.FailureResult("Category cannot be its own parent");

                var parentExists = await _context.ProductCategories
                    .AnyAsync(c => c.Id == dto.ParentCategoryId.Value);
                
                if (!parentExists)
                    return ServiceResult<ProductCategoryDto>.FailureResult("Parent category not found");

                category.ParentCategoryId = dto.ParentCategoryId;
            }

            if (!string.IsNullOrWhiteSpace(dto.CategoryName))
                category.CategoryName = dto.CategoryName;

            if (dto.Description != null)
                category.Description = dto.Description;

            if (dto.ImageUrl != null)
                category.ImageUrl = dto.ImageUrl;

            if (dto.DisplayOrder.HasValue)
                category.DisplayOrder = dto.DisplayOrder.Value;

            if (dto.IsActive.HasValue)
                category.IsActive = dto.IsActive.Value;

            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<ProductCategoryDto>(category);
            resultDto.ProductCount = category.Products.Count;

            return ServiceResult<ProductCategoryDto>.SuccessResult(resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category: {CategoryId}", categoryId);
            return ServiceResult<ProductCategoryDto>.FailureResult($"Error updating category: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteCategoryAsync(Guid categoryId)
    {
        try
        {
            var category = await _context.ProductCategories
                .Include(c => c.Products)
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
                return ServiceResult<bool>.FailureResult("Category not found");

            if (category.Products.Any())
                return ServiceResult<bool>.FailureResult("Cannot delete category with existing products");

            if (category.SubCategories.Any())
                return ServiceResult<bool>.FailureResult("Cannot delete category with subcategories");

            _context.ProductCategories.Remove(category);
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category: {CategoryId}", categoryId);
            return ServiceResult<bool>.FailureResult($"Error deleting category: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> ToggleActiveStatusAsync(Guid categoryId)
    {
        try
        {
            var category = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
                return ServiceResult<bool>.FailureResult("Category not found");

            category.IsActive = !category.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling category status: {CategoryId}", categoryId);
            return ServiceResult<bool>.FailureResult($"Error toggling category status: {ex.Message}");
        }
    }
}


