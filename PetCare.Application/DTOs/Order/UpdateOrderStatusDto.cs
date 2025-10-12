namespace PetCare.Application.DTOs.Order;

public class UpdateOrderStatusDto
{
    public string OrderStatus { get; set; } = string.Empty;
    public string? Note { get; set; }
}
