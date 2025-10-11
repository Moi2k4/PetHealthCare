namespace PetCare.Application.DTOs.Appointment;

public class AppointmentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? PetName { get; set; }
    public string? ServiceName { get; set; }
    public string AppointmentType { get; set; } = string.Empty;
    public string AppointmentStatus { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public string? StaffName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? ServiceAddress { get; set; }
    public string? Notes { get; set; }
}
