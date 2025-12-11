using System.ComponentModel.DataAnnotations;

namespace PetCare.Application.DTOs.Appointment;

public class CreateAppointmentDto
{
    public Guid? PetId { get; set; }
    
    [Required]
    public Guid ServiceId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string AppointmentType { get; set; } = string.Empty;
    
    public Guid? BranchId { get; set; }
    
    [Required]
    public DateTime AppointmentDate { get; set; }
    
    [Required]
    public TimeSpan StartTime { get; set; }
    
    [Required]
    public TimeSpan EndTime { get; set; }
    
    public string? ServiceAddress { get; set; }
    
    public string? Notes { get; set; }
}

public class UpdateAppointmentDto
{
    public Guid? PetId { get; set; }
    public Guid? ServiceId { get; set; }
    public string? AppointmentType { get; set; }
    public Guid? BranchId { get; set; }
    public DateTime? AppointmentDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string? ServiceAddress { get; set; }
    public string? Notes { get; set; }
}

public class AppointmentResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid? PetId { get; set; }
    public string? PetName { get; set; }
    public Guid? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public decimal? ServicePrice { get; set; }
    public string AppointmentType { get; set; } = string.Empty;
    public string AppointmentStatus { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public Guid? AssignedStaffId { get; set; }
    public string? AssignedStaffName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? ServiceAddress { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
