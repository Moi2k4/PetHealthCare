using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using PetCare.Application.Common;
using PetCare.Application.DTOs.Subscription;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Domain.Interfaces;
using PetCare.Infrastructure.Repositories.Interfaces;

namespace PetCare.Application.Services.Implementations;

public class SubscriptionService : ISubscriptionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly PayOSClient _payOS;
    private readonly string _returnUrl;
    private readonly string _cancelUrl;

    public SubscriptionService(IUnitOfWork unitOfWork, IConfiguration configuration, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;

        var clientId = GetFirstNonEmpty(
            configuration["PayOS:ClientId"],
            Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID"))
            ?? throw new InvalidOperationException("PayOS ClientId not configured.");

        var apiKey = GetFirstNonEmpty(
            configuration["PayOS:ApiKey"],
            Environment.GetEnvironmentVariable("PAYOS_API_KEY"))
            ?? throw new InvalidOperationException("PayOS ApiKey not configured.");

        var checksumKey = GetFirstNonEmpty(
            configuration["PayOS:ChecksumKey"],
            Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY"))
            ?? throw new InvalidOperationException("PayOS ChecksumKey not configured.");

        _payOS = new PayOSClient(clientId, apiKey, checksumKey);

        _returnUrl = configuration["PayOS:ReturnUrl"] ?? "https://yourfrontend.com/subscription/success";
        _cancelUrl = configuration["PayOS:CancelUrl"] ?? "https://yourfrontend.com/subscription/cancel";
    }

    public async Task<ServiceResult<IEnumerable<SubscriptionPackageDto>>> GetPackagesAsync()
    {
        try
        {
            var packageRepo = _unitOfWork.Repository<SubscriptionPackage>();

            var packages = await packageRepo
                .QueryWithIncludes()
                .Where(p => p.IsActive)
                .OrderBy(p => p.Price)
                .ToListAsync();

            if (packages.Count == 0)
            {
                var now = DateTime.UtcNow;
                var defaults = new List<SubscriptionPackage>
                {
                    new()
                    {
                        Name = "Free",
                        Description = "Goi co ban de bat dau cham soc thu cung.",
                        Price = 0,
                        BillingCycle = "Month",
                        IsActive = true,
                        HasHealthReminders = true,
                        CreatedAt = now,
                        UpdatedAt = now
                    },
                    new()
                    {
                        Name = "5K",
                        Description = "Goi nang cao voi day du tinh nang chinh o muc phi 5.000 VND.",
                        Price = 5000,
                        BillingCycle = "Month",
                        IsActive = true,
                        HasAIHealthTracking = true,
                        HasVaccinationTracking = true,
                        HasHealthReminders = true,
                        HasAIRecommendations = true,
                        HasNutritionalAnalysis = true,
                        HasEarlyDiseaseDetection = true,
                        HasPrioritySupport = true,
                        CreatedAt = now,
                        UpdatedAt = now
                    }
                };

                foreach (var package in defaults)
                {
                    await packageRepo.AddAsync(package);
                }

                await _unitOfWork.SaveChangesAsync();

                packages = await packageRepo
                    .QueryWithIncludes()
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Price)
                    .ToListAsync();
            }

            var dtos = packages.Select(MapToPackageDto);
            return ServiceResult<IEnumerable<SubscriptionPackageDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<SubscriptionPackageDto>>.FailureResult(
                $"Error loading subscription packages: {ex.Message}");
        }
    }

    public async Task<ServiceResult<SubscriptionPaymentLinkDto>> CreatePaymentLinkAsync(
        CreateSubscriptionPaymentDto dto, Guid userId)
    {
        try
        {
            var package = await _unitOfWork.Repository<SubscriptionPackage>()
                .GetByIdAsync(dto.PackageId);

            if (package == null || !package.IsActive)
                return ServiceResult<SubscriptionPaymentLinkDto>.FailureResult("Subscription package not found.");

            if (package.Price == 0)
            {
                var freeSub = new UserSubscription
                {
                    UserId = userId,
                    SubscriptionPackageId = package.Id,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    NextBillingDate = DateTime.UtcNow.AddMonths(1),
                    IsActive = true,
                    Status = "Active",
                    AmountPaid = 0,
                    PaymentMethod = "free"
                };
                await _unitOfWork.Repository<UserSubscription>().AddAsync(freeSub);
                await _unitOfWork.SaveChangesAsync();

                await SendSubscriptionActivationEmailAsync(userId, package.Name, freeSub.EndDate);

                return ServiceResult<SubscriptionPaymentLinkDto>.SuccessResult(new SubscriptionPaymentLinkDto
                {
                    PaymentUrl = string.Empty,
                    OrderCode = 0,
                    QrCode = string.Empty,
                    PendingSubscriptionId = freeSub.Id
                });
            }

            var orderCode = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var pendingSubscription = new UserSubscription
            {
                UserId = userId,
                SubscriptionPackageId = package.Id,
                StartDate = DateTime.UtcNow,
                EndDate = null,
                IsActive = false,
                Status = "Pending",
                AmountPaid = package.Price,
                PaymentMethod = "payos",
                TransactionId = orderCode.ToString()
            };

            await _unitOfWork.Repository<UserSubscription>().AddAsync(pendingSubscription);
            await _unitOfWork.SaveChangesAsync();

            var amountVnd = (int)Math.Round(package.Price);

            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = amountVnd,
                Description = $"{package.BillingCycle} - {package.Name}",
                ReturnUrl = _returnUrl,
                CancelUrl = _cancelUrl,
                Items = new List<PaymentLinkItem>
                {
                    new PaymentLinkItem { Name = package.Name, Quantity = 1, Price = amountVnd }
                }
            };

            var link = await _payOS.PaymentRequests.CreateAsync(paymentRequest);

            return ServiceResult<SubscriptionPaymentLinkDto>.SuccessResult(new SubscriptionPaymentLinkDto
            {
                PaymentUrl = link.CheckoutUrl,
                OrderCode = orderCode,
                QrCode = link.QrCode,
                PendingSubscriptionId = pendingSubscription.Id
            });
        }
        catch (Exception ex)
        {
            return ServiceResult<SubscriptionPaymentLinkDto>.FailureResult(
                $"Error creating PayOS payment link: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> HandlePayOSWebhookAsync(PayOSWebhookDto webhook)
    {
        try
        {
            var sdkWebhook = new Webhook
            {
                Code = webhook.Code,
                Description = webhook.Desc,
                Success = webhook.Success,
                Signature = webhook.Signature,
                Data = webhook.Data == null ? null : new WebhookData
                {
                    OrderCode = webhook.Data.OrderCode,
                    Amount = webhook.Data.Amount,
                    Description = webhook.Data.Description,
                    AccountNumber = webhook.Data.AccountNumber,
                    Reference = webhook.Data.Reference,
                    TransactionDateTime = webhook.Data.TransactionDateTime,
                    Currency = webhook.Data.Currency,
                    PaymentLinkId = webhook.Data.PaymentLinkId,
                    Code = webhook.Data.Code,
                    CounterAccountBankId = webhook.Data.CounterAccountBankId,
                    CounterAccountBankName = webhook.Data.CounterAccountBankName,
                    CounterAccountName = webhook.Data.CounterAccountName,
                    CounterAccountNumber = webhook.Data.CounterAccountNumber,
                    VirtualAccountName = webhook.Data.VirtualAccountName,
                    VirtualAccountNumber = webhook.Data.VirtualAccountNumber
                }
            };

            var verified = await _payOS.Webhooks.VerifyAsync(sdkWebhook);

            if (verified.Code != "00" || !webhook.Success)
                return ServiceResult<bool>.SuccessResult(false);

            var orderCode = verified.OrderCode.ToString();

            var subscription = await _unitOfWork.Repository<UserSubscription>()
                .QueryWithIncludes(s => s.SubscriptionPackage)
                .FirstOrDefaultAsync(s => s.TransactionId == orderCode && s.Status == "Pending");

            if (subscription == null)
                return ServiceResult<bool>.FailureResult("Pending subscription not found for this order.");

            subscription.IsActive = true;
            subscription.Status = "Active";
            subscription.StartDate = DateTime.UtcNow;
            subscription.EndDate = subscription.SubscriptionPackage.BillingCycle == "Year"
                ? DateTime.UtcNow.AddYears(1)
                : DateTime.UtcNow.AddMonths(1);
            subscription.NextBillingDate = subscription.EndDate;

            await _unitOfWork.Repository<UserSubscription>().UpdateAsync(subscription);
            await _unitOfWork.SaveChangesAsync();

            await SendSubscriptionActivationEmailAsync(
                subscription.UserId,
                subscription.SubscriptionPackage?.Name ?? "Subscription",
                subscription.EndDate);

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Webhook processing error: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> ConfirmPaymentAsync(long orderCode, Guid userId)
    {
        try
        {
            var orderCodeText = orderCode.ToString();

            var pendingSubscription = await _unitOfWork.Repository<UserSubscription>()
                .QueryWithIncludes(s => s.SubscriptionPackage)
                .FirstOrDefaultAsync(s =>
                    s.UserId == userId
                    && s.TransactionId == orderCodeText
                    && s.Status == "Pending");

            if (pendingSubscription == null)
            {
                var activeSubscription = await _unitOfWork.Repository<UserSubscription>()
                    .QueryWithIncludes(s => s.SubscriptionPackage)
                    .FirstOrDefaultAsync(s =>
                        s.UserId == userId
                        && s.TransactionId == orderCodeText
                        && s.IsActive
                        && s.Status == "Active");

                return activeSubscription != null
                    ? ServiceResult<bool>.SuccessResult(true)
                    : ServiceResult<bool>.FailureResult("Pending subscription not found.");
            }

            // Fallback confirmation: activate the pending subscription in return flow.
            // Webhook should still be configured in PayOS dashboard for full reliability.
            pendingSubscription.IsActive = true;
            pendingSubscription.Status = "Active";
            pendingSubscription.StartDate = DateTime.UtcNow;
            pendingSubscription.EndDate = pendingSubscription.SubscriptionPackage.BillingCycle == "Year"
                ? DateTime.UtcNow.AddYears(1)
                : DateTime.UtcNow.AddMonths(1);
            pendingSubscription.NextBillingDate = pendingSubscription.EndDate;

            await _unitOfWork.Repository<UserSubscription>().UpdateAsync(pendingSubscription);
            await _unitOfWork.SaveChangesAsync();

            await SendSubscriptionActivationEmailAsync(
                pendingSubscription.UserId,
                pendingSubscription.SubscriptionPackage?.Name ?? "Subscription",
                pendingSubscription.EndDate);

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Confirm payment error: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserSubscriptionDto?>> GetMySubscriptionAsync(Guid userId)
    {
        try
        {
            var sub = await _unitOfWork.Repository<UserSubscription>()
                .QueryWithIncludes(s => s.SubscriptionPackage)
                .Where(s => s.UserId == userId && s.IsActive && s.Status == "Active")
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            if (sub == null)
                return ServiceResult<UserSubscriptionDto?>.SuccessResult(null);

            return ServiceResult<UserSubscriptionDto?>.SuccessResult(MapToUserSubDto(sub));
        }
        catch (Exception ex)
        {
            return ServiceResult<UserSubscriptionDto?>.FailureResult(
                $"Error retrieving subscription: {ex.Message}");
        }
    }

    public async Task<ServiceResult<MembershipStatusDto>> GetMembershipStatusAsync(Guid userId)
    {
        try
        {
            var now = DateTime.UtcNow;

            var sub = await _unitOfWork.Repository<UserSubscription>()
                .QueryWithIncludes(s => s.SubscriptionPackage)
                .Where(s =>
                    s.UserId == userId
                    && s.IsActive
                    && s.Status == "Active"
                    && (s.EndDate == null || s.EndDate >= now))
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            if (sub == null)
            {
                return ServiceResult<MembershipStatusDto>.SuccessResult(new MembershipStatusDto
                {
                    HasMembership = false
                });
            }

            return ServiceResult<MembershipStatusDto>.SuccessResult(new MembershipStatusDto
            {
                HasMembership = true,
                SubscriptionId = sub.Id,
                PackageName = sub.SubscriptionPackage?.Name,
                StartDate = sub.StartDate,
                EndDate = sub.EndDate,
                Status = sub.Status
            });
        }
        catch (Exception ex)
        {
            return ServiceResult<MembershipStatusDto>.FailureResult(
                $"Error checking membership status: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> CancelSubscriptionAsync(Guid userId)
    {
        try
        {
            var sub = await _unitOfWork.Repository<UserSubscription>()
                .QueryWithIncludes()
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive && s.Status == "Active");

            if (sub == null)
                return ServiceResult<bool>.FailureResult("No active subscription found.");

            sub.IsActive = false;
            sub.Status = "Cancelled";

            await _unitOfWork.Repository<UserSubscription>().UpdateAsync(sub);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error cancelling subscription: {ex.Message}");
        }
    }

    private static SubscriptionPackageDto MapToPackageDto(SubscriptionPackage p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        BillingCycle = p.BillingCycle,
        IsActive = p.IsActive,
        Features = new Dictionary<string, bool>
        {
            ["AIHealthTracking"] = p.HasAIHealthTracking,
            ["VaccinationTracking"] = p.HasVaccinationTracking,
            ["HealthReminders"] = p.HasHealthReminders,
            ["AIRecommendations"] = p.HasAIRecommendations,
            ["NutritionalAnalysis"] = p.HasNutritionalAnalysis,
            ["EarlyDiseaseDetection"] = p.HasEarlyDiseaseDetection,
            ["PrioritySupport"] = p.HasPrioritySupport
        }
    };

    private static UserSubscriptionDto MapToUserSubDto(UserSubscription s) => new()
    {
        Id = s.Id,
        UserId = s.UserId,
        SubscriptionPackageId = s.SubscriptionPackageId,
        PackageName = s.SubscriptionPackage?.Name ?? string.Empty,
        StartDate = s.StartDate,
        EndDate = s.EndDate,
        IsActive = s.IsActive,
        Status = s.Status,
        NextBillingDate = s.NextBillingDate,
        AmountPaid = s.AmountPaid
    };

    private static string? GetFirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private async Task SendSubscriptionActivationEmailAsync(Guid userId, string packageName, DateTime? endDate)
    {
        try
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
            {
                return;
            }

            await _emailService.SendSubscriptionConfirmationAsync(
                user.Email,
                string.IsNullOrWhiteSpace(user.FullName) ? "PetCare User" : user.FullName,
                packageName,
                endDate ?? DateTime.UtcNow.AddMonths(1));
        }
        catch
        {
            // Best effort only: do not fail payment/subscription activation because of email errors.
        }
    }
}
