namespace PetCare.Application.DTOs.Category;

public class ProductCategoryDto
{
    public Guid Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ProductCategoryDto>? SubCategories { get; set; }
    public int ProductCount { get; set; }
}

public class CreateProductCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; } = 0;
}

public class UpdateProductCategoryDto
{
    public string? CategoryName { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
}
