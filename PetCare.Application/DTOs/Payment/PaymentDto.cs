namespace PetCare.Application.DTOs.Payment;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePaymentDto
{
    public Guid OrderId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ReturnUrl { get; set; }
}

public class PaymentCallbackDto
{
    public string PaymentMethod { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? OrderId { get; set; }
    public Dictionary<string, string> AdditionalData { get; set; } = new();
}

public class PaymentUrlResponseDto
{
    public string PaymentUrl { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public Guid PaymentId { get; set; }
}

public class RefundPaymentDto
{
    public Guid PaymentId { get; set; }
    public decimal? RefundAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
}
