using System.ComponentModel.DataAnnotations;

namespace PetCare.Application.DTOs.Product;

public class UpdateProductDto
{
    [MaxLength(255)]
    public string? ProductName { get; set; }

    public string? Description { get; set; }

    public Guid? CategoryId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? Price { get; set; }

    public decimal? SalePrice { get; set; }

    [Range(0, int.MaxValue)]
    public int? StockQuantity { get; set; }

    [MaxLength(100)]
    public string? Sku { get; set; }

    public decimal? Weight { get; set; }
    
    [MaxLength(50)]
    public string? Dimensions { get; set; }
    
    public bool? IsActive { get; set; }
}
