using AutoMapper;
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

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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
                p => p.Brand!,
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
}

