using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PetCare.Application.DTOs.Product;
using PetCare.Application.Common;
using PetCare.Application.Services.Interfaces;
using PetCare.Infrastructure.Repositories.Interfaces;
using PetCare.Domain.Entities;

namespace PetCare.Application.Services.Implementations;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IImageUploadService _imageUploadService;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper, IImageUploadService imageUploadService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _imageUploadService = imageUploadService;
    }

    public async Task<ServiceResult<ProductDto>> GetProductByIdAsync(Guid productId)
    {
        try
        {
            var product = await _unitOfWork.Products.GetProductWithImagesAsync(productId);
            
            if (product == null)
            {
                return ServiceResult<ProductDto>.FailureResult("Product not found");
            }

            var productDto = _mapper.Map<ProductDto>(product);
            return ServiceResult<ProductDto>.SuccessResult(productDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductDto>.FailureResult($"Error retrieving product: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PagedResult<ProductDto>>> GetProductsAsync(int page, int pageSize)
    {
        try
        {
            (IEnumerable<Product> products, int totalCount) = await _unitOfWork.Products.GetPagedAsync(
                page,
                pageSize,
                filter: p => p.IsActive,
                orderBy: q => q.OrderBy(p => p.ProductName),
                p => p.Category!,
                p => p.Images
            );

            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            
            var pagedResult = new PagedResult<ProductDto>
            {
                Items = productDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<ProductDto>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            return ServiceResult<PagedResult<ProductDto>>.FailureResult($"Error retrieving products: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<ProductDto>>> GetProductsByCategoryAsync(Guid categoryId)
    {
        try
        {
            var products = await _unitOfWork.Products.GetProductsByCategoryAsync(categoryId);
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            
            return ServiceResult<IEnumerable<ProductDto>>.SuccessResult(productDtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<ProductDto>>.FailureResult($"Error retrieving products: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<ProductDto>>> SearchProductsAsync(string searchTerm)
    {
        try
        {
            var products = await _unitOfWork.Products.SearchProductsAsync(searchTerm);
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            
            return ServiceResult<IEnumerable<ProductDto>>.SuccessResult(productDtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<ProductDto>>.FailureResult($"Error searching products: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<ProductDto>>> GetActiveProductsAsync()
    {
        try
        {
            var products = await _unitOfWork.Products.GetActiveProductsAsync();
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            
            return ServiceResult<IEnumerable<ProductDto>>.SuccessResult(productDtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<ProductDto>>.FailureResult($"Error retrieving products: {ex.Message}");
        }
    }


    public async Task<ServiceResult<ProductDto>> CreateProductAsync(CreateProductDto createProductDto)
    {
        try
        {
            // Validate that CategoryId exists to avoid FK constraint violations
            if (createProductDto.CategoryId.HasValue && createProductDto.CategoryId.Value != Guid.Empty)
            {
                var category = await _unitOfWork.Repository<ProductCategory>().GetByIdAsync(createProductDto.CategoryId.Value);
                if (category == null)
                {
                    return ServiceResult<ProductDto>.FailureResult($"Category with ID '{createProductDto.CategoryId}' does not exist.");
                }
            }
            else
            {
                // CategoryId is required
                return ServiceResult<ProductDto>.FailureResult("A valid CategoryId is required.");
            }

            var product = _mapper.Map<Product>(createProductDto);
            
            // Handle Images manually if needed, or assume Mapper handles string -> ProductImage if configured
            // For now, let's manually add images if the mapper doesn't support List<string> -> List<ProductImage> directly
            // Handle Images manually
            if (createProductDto.ImageUrls != null && createProductDto.ImageUrls.Any())
            {
                 int order = 0;
                 foreach(var url in createProductDto.ImageUrls)
                 {
                     string finalUrl = url;
                     // Auto-upload if not from Cloudinary
                     if (!string.IsNullOrEmpty(url) && !url.Contains("cloudinary.com"))
                     {
                         var uploadResult = await _imageUploadService.UploadImageFromUrlAsync(url, "products");
                         if (uploadResult.Success)
                         {
                             finalUrl = uploadResult.Data!;
                         }
                     }

                     product.Images.Add(new ProductImage 
                     { 
                         ImageUrl = finalUrl,
                         DisplayOrder = order++,
                         IsPrimary = order == 1 
                     });
                 }
            }

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            var productDto = _mapper.Map<ProductDto>(product);
            return ServiceResult<ProductDto>.SuccessResult(productDto, "Product created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductDto>.FailureResult($"Error creating product: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProductDto>> UpdateProductAsync(Guid productId, UpdateProductDto updateProductDto)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                return ServiceResult<ProductDto>.FailureResult("Product not found");
            }

            _mapper.Map(updateProductDto, product);

            await _unitOfWork.SaveChangesAsync();

            var productDto = _mapper.Map<ProductDto>(product);
            return ServiceResult<ProductDto>.SuccessResult(productDto, "Product updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductDto>.FailureResult($"Error updating product: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteProductAsync(Guid productId)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                return ServiceResult<bool>.FailureResult("Product not found");
            }

            // Soft delete
            product.IsActive = false;
            await _unitOfWork.Products.UpdateAsync(product);
            
            // Or hard delete if preferred:
            // await _unitOfWork.Products.DeleteAsync(product);
            
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true, "Product deleted successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error deleting product: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProductDto>> UpdateStockAsync(Guid productId, int quantity)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                return ServiceResult<ProductDto>.FailureResult("Product not found");
            }

            product.StockQuantity = quantity;
            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            var productDto = _mapper.Map<ProductDto>(product);
            return ServiceResult<ProductDto>.SuccessResult(productDto, "Stock updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductDto>.FailureResult($"Error updating stock: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProductDto>> AddProductImageAsync(Guid productId, string imageUrl)
    {
        try
        {
            // Just verify product exists without loading all images
            var productExists = await _unitOfWork.Products.AnyAsync(p => p.Id == productId);
            if (!productExists)
            {
                return ServiceResult<ProductDto>.FailureResult("Product not found");
            }

            string finalUrl = imageUrl;
            
            // Auto-upload if not from Cloudinary
            if (!string.IsNullOrEmpty(imageUrl) && !imageUrl.Contains("cloudinary.com"))
            {
                var uploadResult = await _imageUploadService.UploadImageFromUrlAsync(imageUrl, "products");
                if (uploadResult.Success)
                {
                    finalUrl = uploadResult.Data!;
                }
                else
                {
                    return ServiceResult<ProductDto>.FailureResult($"Failed to upload image: {uploadResult.Message}");
                }
            }

            // Get current max display order for this product
            var existingImages = await _unitOfWork.Products
                .Query()
                .Where(p => p.Id == productId)
                .SelectMany(p => p.Images)
                .ToListAsync();
            
            int nextOrder = existingImages.Any() ? existingImages.Max(i => i.DisplayOrder) + 1 : 0;
            bool isPrimary = !existingImages.Any();

            // Create and add the image directly without loading the product
            var productImage = new ProductImage
            {
                ProductId = productId,
                ImageUrl = finalUrl,
                DisplayOrder = nextOrder,
                IsPrimary = isPrimary
            };

            // Add directly to context - no product tracking needed
            _unitOfWork.GetContext().ProductImages.Add(productImage);
            await _unitOfWork.SaveChangesAsync();

            // Now load the complete product with images for the response
            var product = await _unitOfWork.Products.GetProductWithImagesAsync(productId);
            var productDto = _mapper.Map<ProductDto>(product);
            return ServiceResult<ProductDto>.SuccessResult(productDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductDto>.FailureResult($"Error adding product image: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> RemoveProductImageAsync(Guid productId, Guid imageId)
    {
        try
        {
            var product = await _unitOfWork.Products.GetProductWithImagesAsync(productId);
            if (product == null)
            {
                return ServiceResult<bool>.FailureResult("Product not found");
            }

            var image = product.Images.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
            {
                return ServiceResult<bool>.FailureResult("Image not found");
            }

            // Optional: Delete from Cloudinary if it's a Cloudinary image
            if (image.ImageUrl.Contains("cloudinary.com"))
            {
                await _imageUploadService.DeleteImageAsync(image.ImageUrl);
            }

            product.Images.Remove(image);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error removing product image: {ex.Message}");
        }
    }
}

