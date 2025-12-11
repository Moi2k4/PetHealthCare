namespace PetCare.Application.DTOs.Service;

public class ServiceDto
{
    public Guid Id { get; set; }
    public Guid? CategoryId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public bool IsHomeService { get; set; }
    public bool IsActive { get; set; }
    public string? CategoryName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateServiceDto
{
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public bool IsHomeService { get; set; } = false;
}

public class UpdateServiceDto
{
    public string? ServiceName { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? DurationMinutes { get; set; }
    public bool? IsActive { get; set; }
}

public class ServiceCategoryDto
{
    public Guid Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public int ServiceCount { get; set; }
}

public class CreateServiceCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
}
