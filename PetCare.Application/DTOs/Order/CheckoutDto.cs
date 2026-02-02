namespace PetCare.Application.DTOs.Order;

public class CheckoutDto
{
    public string ShippingName { get; set; } = string.Empty;
    public string ShippingPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string? ShippingCity { get; set; }
    public string? ShippingDistrict { get; set; }
    public string? Note { get; set; }
    public string? PaymentMethod { get; set; }
    public string? VoucherCode { get; set; }
}
