using PetCare.Application.Common;
using PetCare.Domain.Entities;

namespace PetCare.Application.Services.Interfaces;

public interface IReviewService
{
    Task<ServiceResult<ProductReview>> CreateProductReviewAsync(Guid productId, Guid userId, int rating, string? comment = null);
    Task<ServiceResult<ServiceReview>> CreateServiceReviewAsync(Guid appointmentId, Guid userId, int rating, string? comment = null);
    Task<ServiceResult<PagedResult<ProductReview>>> GetProductReviewsAsync(Guid productId, int page = 1, int pageSize = 10);
    Task<ServiceResult<PagedResult<ServiceReview>>> GetServiceReviewsAsync(Guid serviceId, int page = 1, int pageSize = 10);
    Task<ServiceResult<bool>> DeleteProductReviewAsync(Guid reviewId, Guid userId);
    Task<ServiceResult<bool>> DeleteServiceReviewAsync(Guid reviewId, Guid userId);
    Task<ServiceResult<double>> GetProductAverageRatingAsync(Guid productId);
    Task<ServiceResult<double>> GetServiceAverageRatingAsync(Guid serviceId);
}
