namespace PetCare.Application.DTOs.Review;

public class ProductReviewDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public int Rating { get; set; }
    public string? ReviewText { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateProductReviewDto
{
    public Guid ProductId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class ServiceReviewDto
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public Guid? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public int Rating { get; set; }
    public string? ReviewText { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateServiceReviewDto
{
    public Guid AppointmentId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
