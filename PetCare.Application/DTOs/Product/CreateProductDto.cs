using System.ComponentModel.DataAnnotations;

namespace PetCare.Application.DTOs.Product;

public class CreateProductDto
{
    [Required(ErrorMessage = "Product name is required")]
    [MaxLength(255)]
    public string ProductName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid? CategoryId { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
    public decimal Price { get; set; }

    public decimal? SalePrice { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be greater than or equal to 0")]
    public int StockQuantity { get; set; } = 0;

    [MaxLength(100)]
    public string? Sku { get; set; }

    public decimal? Weight { get; set; }
    
    [MaxLength(50)]
    public string? Dimensions { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public List<string> ImageUrls { get; set; } = new();
}
