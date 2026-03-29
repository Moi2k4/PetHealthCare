using System.ComponentModel.DataAnnotations;

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

public class DogRoutineScheduleDto
{
    public Guid PetId { get; set; }
    public string PetName { get; set; } = string.Empty;
    public bool IsDog { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Note { get; set; }
    public List<DogRoutineItemDto> Vaccinations { get; set; } = new();
    public List<DogRoutineItemDto> Deworming { get; set; } = new();
}

public class DogRoutineItemDto
{
    public string Category { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime? LastCompletedDate { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}

public class CreateVaccinationDto
{
    [StringLength(50)]
    public string? VaccineCode { get; set; }

    [Required]
    [StringLength(255, MinimumLength = 2)]
    public string VaccineName { get; set; } = string.Empty;

    public DateTime? VaccinationDate { get; set; }
    public DateTime? NextDueDate { get; set; }

    [StringLength(100)]
    public string? BatchNumber { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class VaccineCatalogDto
{
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string[] Aliases { get; set; } = [];
    public int? DefaultIntervalDays { get; set; }
}

public class VaccinationDto
{
    public Guid Id { get; set; }
    public Guid PetId { get; set; }
    public string? VaccineCode { get; set; }
    public string VaccineName { get; set; } = string.Empty;
    public DateTime VaccinationDate { get; set; }
    public DateTime? NextDueDate { get; set; }
    public string? BatchNumber { get; set; }
    public Guid? AdministeredBy { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}


