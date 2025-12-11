using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetCare.Application.Common;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Data;
using System.Text.Json;

namespace PetCare.Application.Services.Implementations;

public class SubscriptionService : ISubscriptionService
{
    private readonly PetCareDbContext _context;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(PetCareDbContext context, ILogger<SubscriptionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceResult<SubscriptionPackage>> CreatePackageAsync(string name, string description, decimal price, string billingCycle, Dictionary<string, bool> features)
    {
        try
        {
            var package = new SubscriptionPackage
            {
                Name = name,
                Description = description,
                Price = price,
                BillingCycle = billingCycle,
                IsActive = true,
                Features = JsonSerializer.Serialize(features)
            };

            // Set feature flags
            if (features.TryGetValue("AIHealthTracking", out var aiHealth))
                package.HasAIHealthTracking = aiHealth;
            if (features.TryGetValue("VaccinationTracking", out var vaccination))
                package.HasVaccinationTracking = vaccination;
            if (features.TryGetValue("HealthReminders", out var reminders))
                package.HasHealthReminders = reminders;
            if (features.TryGetValue("AIRecommendations", out var aiRec))
                package.HasAIRecommendations = aiRec;
            if (features.TryGetValue("NutritionalAnalysis", out var nutrition))
                package.HasNutritionalAnalysis = nutrition;
            if (features.TryGetValue("EarlyDiseaseDetection", out var disease))
                package.HasEarlyDiseaseDetection = disease;
            if (features.TryGetValue("PrioritySupport", out var support))
                package.HasPrioritySupport = support;

            _context.SubscriptionPackages.Add(package);
            await _context.SaveChangesAsync();

            return ServiceResult<SubscriptionPackage>.SuccessResult(package);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription package");
            return ServiceResult<SubscriptionPackage>.FailureResult($"Error creating package: {ex.Message}");
        }
    }

    public async Task<ServiceResult<SubscriptionPackage>> UpdatePackageAsync(Guid id, string? name = null, string? description = null, decimal? price = null)
    {
        try
        {
            var package = await _context.SubscriptionPackages.FindAsync(id);
            if (package == null)
                return ServiceResult<SubscriptionPackage>.FailureResult("Package not found");

            if (!string.IsNullOrWhiteSpace(name))
                package.Name = name;

            if (description != null)
                package.Description = description;

            if (price.HasValue)
                package.Price = price.Value;

            package.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ServiceResult<SubscriptionPackage>.SuccessResult(package);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating package: {PackageId}", id);
            return ServiceResult<SubscriptionPackage>.FailureResult($"Error updating package: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeactivatePackageAsync(Guid id)
    {
        try
        {
            var package = await _context.SubscriptionPackages.FindAsync(id);
            if (package == null)
                return ServiceResult<bool>.FailureResult("Package not found");

            package.IsActive = false;
            package.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating package: {PackageId}", id);
            return ServiceResult<bool>.FailureResult($"Error deactivating package: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<SubscriptionPackage>>> GetAllPackagesAsync(bool activeOnly = true)
    {
        try
        {
            var query = _context.SubscriptionPackages.AsQueryable();

            if (activeOnly)
                query = query.Where(p => p.IsActive);

            var packages = await query.OrderBy(p => p.Price).ToListAsync();

            return ServiceResult<List<SubscriptionPackage>>.SuccessResult(packages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting packages");
            return ServiceResult<List<SubscriptionPackage>>.FailureResult($"Error retrieving packages: {ex.Message}");
        }
    }

    public async Task<ServiceResult<SubscriptionPackage>> GetPackageByIdAsync(Guid id)
    {
        try
        {
            var package = await _context.SubscriptionPackages.FindAsync(id);
            if (package == null)
                return ServiceResult<SubscriptionPackage>.FailureResult("Package not found");

            return ServiceResult<SubscriptionPackage>.SuccessResult(package);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting package: {PackageId}", id);
            return ServiceResult<SubscriptionPackage>.FailureResult($"Error retrieving package: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserSubscription>> SubscribeAsync(Guid userId, Guid packageId, string paymentMethod, string? transactionId = null)
    {
        try
        {
            var package = await _context.SubscriptionPackages.FindAsync(packageId);
            if (package == null || !package.IsActive)
                return ServiceResult<UserSubscription>.FailureResult("Package not found or inactive");

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                return ServiceResult<UserSubscription>.FailureResult("User not found");

            // Cancel any existing active subscription
            var existingSubscription = await _context.UserSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

            if (existingSubscription != null)
            {
                existingSubscription.IsActive = false;
                existingSubscription.Status = "Cancelled";
                existingSubscription.EndDate = DateTime.UtcNow;
            }

            // Create new subscription
            var subscription = new UserSubscription
            {
                UserId = userId,
                SubscriptionPackageId = packageId,
                StartDate = DateTime.UtcNow,
                IsActive = true,
                Status = "Active",
                AmountPaid = package.Price,
                PaymentMethod = paymentMethod,
                TransactionId = transactionId,
                NextBillingDate = package.BillingCycle == "Year" 
                    ? DateTime.UtcNow.AddYears(1) 
                    : DateTime.UtcNow.AddMonths(1)
            };

            _context.UserSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            var result = await _context.UserSubscriptions
                .Include(s => s.SubscriptionPackage)
                .FirstOrDefaultAsync(s => s.Id == subscription.Id);

            return ServiceResult<UserSubscription>.SuccessResult(result!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing user: {UserId} to package: {PackageId}", userId, packageId);
            return ServiceResult<UserSubscription>.FailureResult($"Error creating subscription: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserSubscription>> CancelSubscriptionAsync(Guid userId, Guid subscriptionId)
    {
        try
        {
            var subscription = await _context.UserSubscriptions
                .Include(s => s.SubscriptionPackage)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.UserId == userId);

            if (subscription == null)
                return ServiceResult<UserSubscription>.FailureResult("Subscription not found");

            subscription.IsActive = false;
            subscription.Status = "Cancelled";
            subscription.EndDate = DateTime.UtcNow;
            subscription.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ServiceResult<UserSubscription>.SuccessResult(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription: {SubscriptionId}", subscriptionId);
            return ServiceResult<UserSubscription>.FailureResult($"Error cancelling subscription: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserSubscription>> GetUserActiveSubscriptionAsync(Guid userId)
    {
        try
        {
            var subscription = await _context.UserSubscriptions
                .Include(s => s.SubscriptionPackage)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

            if (subscription == null)
                return ServiceResult<UserSubscription>.FailureResult("No active subscription found");

            return ServiceResult<UserSubscription>.SuccessResult(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active subscription for user: {UserId}", userId);
            return ServiceResult<UserSubscription>.FailureResult($"Error retrieving subscription: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<UserSubscription>>> GetUserSubscriptionHistoryAsync(Guid userId)
    {
        try
        {
            var subscriptions = await _context.UserSubscriptions
                .Include(s => s.SubscriptionPackage)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();

            return ServiceResult<List<UserSubscription>>.SuccessResult(subscriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription history for user: {UserId}", userId);
            return ServiceResult<List<UserSubscription>>.FailureResult($"Error retrieving subscription history: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> CheckUserHasFeatureAsync(Guid userId, string featureName)
    {
        try
        {
            var subscription = await _context.UserSubscriptions
                .Include(s => s.SubscriptionPackage)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

            if (subscription == null)
                return ServiceResult<bool>.SuccessResult(false);

            var package = subscription.SubscriptionPackage;
            var hasFeature = featureName switch
            {
                "AIHealthTracking" => package.HasAIHealthTracking,
                "VaccinationTracking" => package.HasVaccinationTracking,
                "HealthReminders" => package.HasHealthReminders,
                "AIRecommendations" => package.HasAIRecommendations,
                "NutritionalAnalysis" => package.HasNutritionalAnalysis,
                "EarlyDiseaseDetection" => package.HasEarlyDiseaseDetection,
                "PrioritySupport" => package.HasPrioritySupport,
                _ => false
            };

            return ServiceResult<bool>.SuccessResult(hasFeature);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature for user: {UserId}", userId);
            return ServiceResult<bool>.FailureResult($"Error checking feature: {ex.Message}");
        }
    }
}


