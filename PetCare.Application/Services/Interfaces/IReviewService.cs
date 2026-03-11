using PetCare.Application.Common;
using PetCare.Application.DTOs.Review;

namespace PetCare.Application.Services.Interfaces;

public interface IReviewService
{
    // Product reviews
    Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetProductReviewsAsync(Guid productId);
    Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetMyProductReviewsAsync(Guid userId);
    Task<ServiceResult<ProductReviewDto>> CreateProductReviewAsync(Guid userId, CreateProductReviewDto dto);
    Task<ServiceResult<bool>> ApproveProductReviewAsync(Guid reviewId);
    Task<ServiceResult<bool>> DeleteProductReviewAsync(Guid reviewId);

    // Service reviews
    Task<ServiceResult<IEnumerable<ServiceReviewDto>>> GetServiceReviewsAsync(Guid serviceId);
    Task<ServiceResult<IEnumerable<ServiceReviewDto>>> GetMyServiceReviewsAsync(Guid userId);
    Task<ServiceResult<ServiceReviewDto>> CreateServiceReviewAsync(Guid userId, CreateServiceReviewDto dto);
    Task<ServiceResult<bool>> ApproveServiceReviewAsync(Guid reviewId);
    Task<ServiceResult<bool>> DeleteServiceReviewAsync(Guid reviewId);
}
