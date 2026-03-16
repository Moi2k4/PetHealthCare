using PetCare.Application.Common;
using PetCare.Application.DTOs.Subscription;

namespace PetCare.Application.Services.Interfaces;

public interface ISubscriptionService
{
    /// <summary>Returns all available (active) subscription packages.</summary>
    Task<ServiceResult<IEnumerable<SubscriptionPackageDto>>> GetPackagesAsync();

    /// <summary>Creates a PayOS payment link for the chosen package.</summary>
    Task<ServiceResult<SubscriptionPaymentLinkDto>> CreatePaymentLinkAsync(
        CreateSubscriptionPaymentDto dto, Guid userId);

    /// <summary>Handles the PayOS webhook and activates the subscription on success.</summary>
    Task<ServiceResult<bool>> HandlePayOSWebhookAsync(PayOSWebhookDto webhook);

    /// <summary>Confirms payment from return flow and activates pending subscription if paid.</summary>
    Task<ServiceResult<bool>> ConfirmPaymentAsync(long orderCode, Guid userId);

    /// <summary>Returns the active subscription for a user (null if none).</summary>
    Task<ServiceResult<UserSubscriptionDto?>> GetMySubscriptionAsync(Guid userId);

    /// <summary>Returns membership status for a user account.</summary>
    Task<ServiceResult<MembershipStatusDto>> GetMembershipStatusAsync(Guid userId);

    /// <summary>Cancels the user's active subscription.</summary>
    Task<ServiceResult<bool>> CancelSubscriptionAsync(Guid userId);
}
