namespace PetCare.Application.DTOs.Order;

public class CreateOrderDto
{
    public string ShippingName { get; set; } = string.Empty;
    public string ShippingPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string? ShippingCity { get; set; }
    public string? ShippingDistrict { get; set; }
    public string? Note { get; set; }
    public string? PaymentMethod { get; set; }
    public List<OrderItemRequestDto> Items { get; set; } = new();
}

public class OrderItemRequestDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
