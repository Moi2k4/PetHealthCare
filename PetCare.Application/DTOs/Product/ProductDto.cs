namespace PetCare.Application.DTOs.Product;

public class ProductDto
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public int StockQuantity { get; set; }
    public string? Sku { get; set; }
    public string? CategoryName { get; set; }
    public string? BrandName { get; set; }
    public List<string> Images { get; set; } = new();
    public bool IsActive { get; set; }
}
