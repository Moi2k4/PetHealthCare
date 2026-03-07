namespace PetCare.Application.DTOs.Health;

public class HealthRecordDto
{
    public Guid Id { get; set; }
    public Guid PetId { get; set; }
    public string? PetName { get; set; }
    public DateTime RecordDate { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public decimal? Temperature { get; set; }
    public int? HeartRate { get; set; }
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public string? Notes { get; set; }
    public Guid? RecordedBy { get; set; }
    public string? RecordedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateHealthRecordDto
{
    public Guid PetId { get; set; }
    public DateTime RecordDate { get; set; } = DateTime.UtcNow;
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public decimal? Temperature { get; set; }
    public int? HeartRate { get; set; }
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public string? Notes { get; set; }
}

public class UpdateHealthRecordDto
{
    public DateTime? RecordDate { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public decimal? Temperature { get; set; }
    public int? HeartRate { get; set; }
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public string? Notes { get; set; }
}


