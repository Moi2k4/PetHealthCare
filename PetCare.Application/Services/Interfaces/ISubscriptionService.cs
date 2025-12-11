using PetCare.Application.Common;
using PetCare.Domain.Entities;

namespace PetCare.Application.Services.Interfaces;

public interface ISubscriptionService
{
    Task<ServiceResult<SubscriptionPackage>> CreatePackageAsync(string name, string description, decimal price, string billingCycle, Dictionary<string, bool> features);
    Task<ServiceResult<SubscriptionPackage>> UpdatePackageAsync(Guid id, string? name = null, string? description = null, decimal? price = null);
    Task<ServiceResult<bool>> DeactivatePackageAsync(Guid id);
    Task<ServiceResult<List<SubscriptionPackage>>> GetAllPackagesAsync(bool activeOnly = true);
    Task<ServiceResult<SubscriptionPackage>> GetPackageByIdAsync(Guid id);
    Task<ServiceResult<UserSubscription>> SubscribeAsync(Guid userId, Guid packageId, string paymentMethod, string? transactionId = null);
    Task<ServiceResult<UserSubscription>> CancelSubscriptionAsync(Guid userId, Guid subscriptionId);
    Task<ServiceResult<UserSubscription>> GetUserActiveSubscriptionAsync(Guid userId);
    Task<ServiceResult<List<UserSubscription>>> GetUserSubscriptionHistoryAsync(Guid userId);
    Task<ServiceResult<bool>> CheckUserHasFeatureAsync(Guid userId, string featureName);
}
