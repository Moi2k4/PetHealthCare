namespace PetCare.Application.DTOs.Subscription;

public class CreateSubscriptionPaymentDto
{
    /// <summary>ID of the subscription package the user wants to purchase.</summary>
    public Guid PackageId { get; set; }
}

public class SubscriptionPaymentLinkDto
{
    public string PaymentUrl { get; set; } = string.Empty;
    public long OrderCode { get; set; }
    public string QrCode { get; set; } = string.Empty;
    public Guid PendingSubscriptionId { get; set; }
}

public class PayOSWebhookDto
{
    public string Code { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
    public bool Success { get; set; }
    public PayOSWebhookDataDto? Data { get; set; }
    public string Signature { get; set; } = string.Empty;
}

public class PayOSWebhookDataDto
{
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string TransactionDateTime { get; set; } = string.Empty;
    public string PaymentLinkId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
    public string CounterAccountBankId { get; set; } = string.Empty;
    public string CounterAccountBankName { get; set; } = string.Empty;
    public string CounterAccountName { get; set; } = string.Empty;
    public string CounterAccountNumber { get; set; } = string.Empty;
    public string VirtualAccountName { get; set; } = string.Empty;
    public string VirtualAccountNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
}
