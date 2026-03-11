using PetCare.Application.Common;
using PetCare.Application.DTOs.Review;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Repositories.Interfaces;

namespace PetCare.Application.Services.Implementations;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReviewService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ──────────────────────── Product Reviews ────────────────────────

    public async Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetProductReviewsAsync(Guid productId)
    {
        try
        {
            var reviews = await _unitOfWork.Repository<ProductReview>()
                .FindAsync(r => r.ProductId == productId && r.IsApproved);

            var userRepo = _unitOfWork.Repository<User>();
            var dtos = new List<ProductReviewDto>();
            foreach (var r in reviews.OrderByDescending(r => r.CreatedAt))
            {
                var user = r.UserId.HasValue ? await userRepo.GetByIdAsync(r.UserId.Value) : null;
                dtos.Add(MapProductReview(r, user));
            }

            return ServiceResult<IEnumerable<ProductReviewDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<ProductReviewDto>>.FailureResult($"Error retrieving reviews: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetMyProductReviewsAsync(Guid userId)
    {
        try
        {
            var reviews = await _unitOfWork.Repository<ProductReview>()
                .FindAsync(r => r.UserId == userId);

            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            var dtos = reviews.OrderByDescending(r => r.CreatedAt)
                .Select(r => MapProductReview(r, user));

            return ServiceResult<IEnumerable<ProductReviewDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<ProductReviewDto>>.FailureResult($"Error retrieving reviews: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProductReviewDto>> CreateProductReviewAsync(Guid userId, CreateProductReviewDto dto)
    {
        try
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                return ServiceResult<ProductReviewDto>.FailureResult("Rating must be between 1 and 5");

            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
                return ServiceResult<ProductReviewDto>.FailureResult("Product not found");

            // Check for duplicate review
            var alreadyReviewed = await _unitOfWork.Repository<ProductReview>()
                .AnyAsync(r => r.ProductId == dto.ProductId && r.UserId == userId);
            if (alreadyReviewed)
                return ServiceResult<ProductReviewDto>.FailureResult("You have already reviewed this product");

            // Verify purchase: user must have a delivered order containing this product
            var orders = await _unitOfWork.Repository<Order>()
                .FindAsync(o => o.UserId == userId && o.OrderStatus == "delivered");

            bool isVerified = false;
            Guid? verifiedOrderId = null;
            foreach (var order in orders)
            {
                var hasItem = await _unitOfWork.Repository<OrderItem>()
                    .AnyAsync(i => i.OrderId == order.Id && i.ProductId == dto.ProductId);
                if (hasItem)
                {
                    isVerified = true;
                    verifiedOrderId = order.Id;
                    break;
                }
            }

            var review = new ProductReview
            {
                ProductId = dto.ProductId,
                UserId = userId,
                OrderId = verifiedOrderId,
                Rating = dto.Rating,
                ReviewText = dto.Comment,
                IsVerifiedPurchase = isVerified,
                IsApproved = false
            };

            await _unitOfWork.Repository<ProductReview>().AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            return ServiceResult<ProductReviewDto>.SuccessResult(MapProductReview(review, user), "Review submitted for approval");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductReviewDto>.FailureResult($"Error creating review: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> ApproveProductReviewAsync(Guid reviewId)
    {
        try
        {
            var review = await _unitOfWork.Repository<ProductReview>().GetByIdAsync(reviewId);
            if (review == null)
                return ServiceResult<bool>.FailureResult("Review not found");

            review.IsApproved = true;
            await _unitOfWork.Repository<ProductReview>().UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult<bool>.SuccessResult(true, "Review approved");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error approving review: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteProductReviewAsync(Guid reviewId)
    {
        try
        {
            var review = await _unitOfWork.Repository<ProductReview>().GetByIdAsync(reviewId);
            if (review == null)
                return ServiceResult<bool>.FailureResult("Review not found");

            await _unitOfWork.Repository<ProductReview>().DeleteAsync(review);
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult<bool>.SuccessResult(true, "Review deleted");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error deleting review: {ex.Message}");
        }
    }

    // ──────────────────────── Service Reviews ────────────────────────

    public async Task<ServiceResult<IEnumerable<ServiceReviewDto>>> GetServiceReviewsAsync(Guid serviceId)
    {
        try
        {
            var reviews = await _unitOfWork.Repository<ServiceReview>()
                .FindAsync(r => r.ServiceId == serviceId && r.IsApproved);

            var userRepo = _unitOfWork.Repository<User>();
            var serviceRepo = _unitOfWork.Repository<Service>();
            var dtos = new List<ServiceReviewDto>();

            foreach (var r in reviews.OrderByDescending(r => r.CreatedAt))
            {
                var user = r.UserId.HasValue ? await userRepo.GetByIdAsync(r.UserId.Value) : null;
                var service = r.ServiceId.HasValue ? await serviceRepo.GetByIdAsync(r.ServiceId.Value) : null;
                dtos.Add(MapServiceReview(r, user, service));
            }

            return ServiceResult<IEnumerable<ServiceReviewDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<ServiceReviewDto>>.FailureResult($"Error retrieving reviews: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<ServiceReviewDto>>> GetMyServiceReviewsAsync(Guid userId)
    {
        try
        {
            var reviews = await _unitOfWork.Repository<ServiceReview>()
                .FindAsync(r => r.UserId == userId);

            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            var serviceRepo = _unitOfWork.Repository<Service>();
            var dtos = new List<ServiceReviewDto>();

            foreach (var r in reviews.OrderByDescending(r => r.CreatedAt))
            {
                var service = r.ServiceId.HasValue ? await serviceRepo.GetByIdAsync(r.ServiceId.Value) : null;
                dtos.Add(MapServiceReview(r, user, service));
            }

            return ServiceResult<IEnumerable<ServiceReviewDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<ServiceReviewDto>>.FailureResult($"Error retrieving reviews: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ServiceReviewDto>> CreateServiceReviewAsync(Guid userId, CreateServiceReviewDto dto)
    {
        try
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                return ServiceResult<ServiceReviewDto>.FailureResult("Rating must be between 1 and 5");

            // Verify appointment belongs to user and is completed
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(dto.AppointmentId);
            if (appointment == null || appointment.UserId != userId)
                return ServiceResult<ServiceReviewDto>.FailureResult("Appointment not found");

            if (appointment.AppointmentStatus != "completed")
                return ServiceResult<ServiceReviewDto>.FailureResult("You can only review a completed appointment");

            // Check for duplicate review
            var alreadyReviewed = await _unitOfWork.Repository<ServiceReview>()
                .AnyAsync(r => r.AppointmentId == dto.AppointmentId && r.UserId == userId);
            if (alreadyReviewed)
                return ServiceResult<ServiceReviewDto>.FailureResult("You have already reviewed this appointment");

            var review = new ServiceReview
            {
                AppointmentId = dto.AppointmentId,
                UserId = userId,
                ServiceId = appointment.ServiceId,
                StaffId = appointment.AssignedStaffId,
                Rating = dto.Rating,
                ReviewText = dto.Comment,
                IsApproved = false
            };

            await _unitOfWork.Repository<ServiceReview>().AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            var service = appointment.ServiceId.HasValue
                ? await _unitOfWork.Repository<Service>().GetByIdAsync(appointment.ServiceId.Value)
                : null;

            return ServiceResult<ServiceReviewDto>.SuccessResult(MapServiceReview(review, user, service), "Review submitted for approval");
        }
        catch (Exception ex)
        {
            return ServiceResult<ServiceReviewDto>.FailureResult($"Error creating review: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> ApproveServiceReviewAsync(Guid reviewId)
    {
        try
        {
            var review = await _unitOfWork.Repository<ServiceReview>().GetByIdAsync(reviewId);
            if (review == null)
                return ServiceResult<bool>.FailureResult("Review not found");

            review.IsApproved = true;
            await _unitOfWork.Repository<ServiceReview>().UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult<bool>.SuccessResult(true, "Review approved");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error approving review: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteServiceReviewAsync(Guid reviewId)
    {
        try
        {
            var review = await _unitOfWork.Repository<ServiceReview>().GetByIdAsync(reviewId);
            if (review == null)
                return ServiceResult<bool>.FailureResult("Review not found");

            await _unitOfWork.Repository<ServiceReview>().DeleteAsync(review);
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult<bool>.SuccessResult(true, "Review deleted");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error deleting review: {ex.Message}");
        }
    }

    // ──────────────────────── Mappers ────────────────────────

    private static ProductReviewDto MapProductReview(ProductReview r, User? user) => new()
    {
        Id = r.Id,
        ProductId = r.ProductId,
        UserId = r.UserId,
        UserName = user?.FullName,
        Rating = r.Rating,
        ReviewText = r.ReviewText,
        IsVerifiedPurchase = r.IsVerifiedPurchase,
        IsApproved = r.IsApproved,
        CreatedAt = r.CreatedAt
    };

    private static ServiceReviewDto MapServiceReview(ServiceReview r, User? user, Service? service) => new()
    {
        Id = r.Id,
        AppointmentId = r.AppointmentId,
        UserId = r.UserId,
        UserName = user?.FullName,
        ServiceId = r.ServiceId,
        ServiceName = service?.ServiceName,
        Rating = r.Rating,
        ReviewText = r.ReviewText,
        IsApproved = r.IsApproved,
        CreatedAt = r.CreatedAt
    };
}
