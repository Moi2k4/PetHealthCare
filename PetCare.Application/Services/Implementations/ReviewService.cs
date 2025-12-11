using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetCare.Application.Common;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Data;

namespace PetCare.Application.Services.Implementations;

public class ReviewService : IReviewService
{
    private readonly PetCareDbContext _context;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(PetCareDbContext context, ILogger<ReviewService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceResult<ProductReview>> CreateProductReviewAsync(Guid productId, Guid userId, int rating, string? comment = null)
    {
        try
        {
            if (rating < 1 || rating > 5)
                return ServiceResult<ProductReview>.FailureResult("Rating must be between 1 and 5");

            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
            if (!productExists)
                return ServiceResult<ProductReview>.FailureResult("Product not found");

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                return ServiceResult<ProductReview>.FailureResult("User not found");

            // Check if user has purchased this product
            var hasPurchased = await _context.OrderItems
                .AnyAsync(oi => oi.ProductId == productId && 
                               oi.Order.UserId == userId &&
                               oi.Order.OrderStatus == "Delivered");

            var review = new ProductReview
            {
                ProductId = productId,
                UserId = userId,
                Rating = rating,
                ReviewText = comment,
                IsVerifiedPurchase = hasPurchased,
                IsApproved = false
            };

            _context.ProductReviews.Add(review);
            await _context.SaveChangesAsync();

            var result = await _context.ProductReviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == review.Id);

            return ServiceResult<ProductReview>.SuccessResult(result!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product review");
            return ServiceResult<ProductReview>.FailureResult($"Error creating review: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ServiceReview>> CreateServiceReviewAsync(Guid appointmentId, Guid userId, int rating, string? comment = null)
    {
        try
        {
            if (rating < 1 || rating > 5)
                return ServiceResult<ServiceReview>.FailureResult("Rating must be between 1 and 5");

            var appointment = await _context.Appointments
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                return ServiceResult<ServiceReview>.FailureResult("Appointment not found");

            if (appointment.UserId != userId)
                return ServiceResult<ServiceReview>.FailureResult("You can only review your own appointments");

            if (appointment.AppointmentStatus != "Completed")
                return ServiceResult<ServiceReview>.FailureResult("Can only review completed appointments");

            // Check if already reviewed
            var existingReview = await _context.ServiceReviews
                .AnyAsync(r => r.AppointmentId == appointmentId);

            if (existingReview)
                return ServiceResult<ServiceReview>.FailureResult("Appointment already reviewed");

            var review = new ServiceReview
            {
                AppointmentId = appointmentId,
                UserId = userId,
                ServiceId = appointment.ServiceId,
                StaffId = appointment.AssignedStaffId,
                Rating = rating,
                ReviewText = comment,
                IsApproved = false
            };

            _context.ServiceReviews.Add(review);
            await _context.SaveChangesAsync();

            var result = await _context.ServiceReviews
                .Include(r => r.User)
                .Include(r => r.Service)
                .FirstOrDefaultAsync(r => r.Id == review.Id);

            return ServiceResult<ServiceReview>.SuccessResult(result!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service review");
            return ServiceResult<ServiceReview>.FailureResult($"Error creating review: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PagedResult<ProductReview>>> GetProductReviewsAsync(Guid productId, int page = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.ProductReviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId && r.IsApproved);

            var totalCount = await query.CountAsync();
            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<ProductReview>
            {
                Items = reviews,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<ProductReview>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product reviews: {ProductId}", productId);
            return ServiceResult<PagedResult<ProductReview>>.FailureResult($"Error retrieving reviews: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PagedResult<ServiceReview>>> GetServiceReviewsAsync(Guid serviceId, int page = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.ServiceReviews
                .Include(r => r.User)
                .Where(r => r.ServiceId == serviceId && r.IsApproved);

            var totalCount = await query.CountAsync();
            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<ServiceReview>
            {
                Items = reviews,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<ServiceReview>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service reviews: {ServiceId}", serviceId);
            return ServiceResult<PagedResult<ServiceReview>>.FailureResult($"Error retrieving reviews: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteProductReviewAsync(Guid reviewId, Guid userId)
    {
        try
        {
            var review = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

            if (review == null)
                return ServiceResult<bool>.FailureResult("Review not found or you don't have permission to delete it");

            _context.ProductReviews.Remove(review);
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product review: {ReviewId}", reviewId);
            return ServiceResult<bool>.FailureResult($"Error deleting review: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteServiceReviewAsync(Guid reviewId, Guid userId)
    {
        try
        {
            var review = await _context.ServiceReviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

            if (review == null)
                return ServiceResult<bool>.FailureResult("Review not found or you don't have permission to delete it");

            _context.ServiceReviews.Remove(review);
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting service review: {ReviewId}", reviewId);
            return ServiceResult<bool>.FailureResult($"Error deleting review: {ex.Message}");
        }
    }

    public async Task<ServiceResult<double>> GetProductAverageRatingAsync(Guid productId)
    {
        try
        {
            var reviews = await _context.ProductReviews
                .Where(r => r.ProductId == productId && r.IsApproved)
                .ToListAsync();

            if (!reviews.Any())
                return ServiceResult<double>.SuccessResult(0);

            var average = reviews.Average(r => r.Rating);
            return ServiceResult<double>.SuccessResult(Math.Round(average, 2));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product average rating: {ProductId}", productId);
            return ServiceResult<double>.FailureResult($"Error calculating rating: {ex.Message}");
        }
    }

    public async Task<ServiceResult<double>> GetServiceAverageRatingAsync(Guid serviceId)
    {
        try
        {
            var reviews = await _context.ServiceReviews
                .Where(r => r.ServiceId == serviceId && r.IsApproved)
                .ToListAsync();

            if (!reviews.Any())
                return ServiceResult<double>.SuccessResult(0);

            var average = reviews.Average(r => r.Rating);
            return ServiceResult<double>.SuccessResult(Math.Round(average, 2));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service average rating: {ServiceId}", serviceId);
            return ServiceResult<double>.FailureResult($"Error calculating rating: {ex.Message}");
        }
    }
}




