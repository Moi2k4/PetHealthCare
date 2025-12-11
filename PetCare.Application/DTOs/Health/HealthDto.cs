namespace PetCare.Application.DTOs.Health;

public class HealthRecordDto
{
    public Guid Id { get; set; }
    public Guid PetId { get; set; }
    public string? PetName { get; set; }
    public DateTime RecordDate { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Temperature { get; set; }
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateHealthRecordDto
{
    public Guid PetId { get; set; }
    public string RecordType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime RecordDate { get; set; }
    public Guid? VeterinarianId { get; set; }
}

public class VaccinationDto
{
    public Guid Id { get; set; }
    public Guid PetId { get; set; }
    public string? PetName { get; set; }
    public string VaccineName { get; set; } = string.Empty;
    public DateTime VaccinationDate { get; set; }
    public DateTime? NextDueDate { get; set; }
    public string? BatchNumber { get; set; }
    public string? Notes { get; set; }
    public bool IsOverdue { get; set; }
}

public class CreateVaccinationDto
{
    public Guid PetId { get; set; }
    public string VaccineName { get; set; } = string.Empty;
    public DateTime VaccinationDate { get; set; }
    public DateTime? NextDueDate { get; set; }
    public string? BatchNumber { get; set; }
}

public class HealthReminderDto
{
    public Guid Id { get; set; }
    public Guid PetId { get; set; }
    public string? PetName { get; set; }
    public string ReminderType { get; set; } = string.Empty;
    public string ReminderTitle { get; set; } = string.Empty;
    public DateTime ReminderDate { get; set; }
    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }
    public bool IsUpcoming { get; set; }
}

public class CreateHealthReminderDto
{
    public Guid PetId { get; set; }
    public string ReminderType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime ReminderDate { get; set; }
    public string? Notes { get; set; }
}
