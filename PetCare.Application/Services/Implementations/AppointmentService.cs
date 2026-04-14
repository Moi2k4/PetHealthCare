using PetCare.Application.Common;
using PetCare.Application.DTOs.Appointment;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Repositories.Interfaces;

namespace PetCare.Application.Services.Implementations;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public AppointmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<IEnumerable<ServiceListItemDto>>> GetAvailableServicesAsync()
    {
        try
        {
            var services = await _unitOfWork.Services.GetActiveServicesAsync();
            var dtos = services.Select(s => new ServiceListItemDto
            {
                Id = s.Id,
                ServiceName = s.ServiceName,
                Description = s.Description,
                Price = s.Price,
                DurationMinutes = s.DurationMinutes,
                IsHomeService = s.IsHomeService,
                CategoryName = s.Category?.CategoryName
            });
            return ServiceResult<IEnumerable<ServiceListItemDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<ServiceListItemDto>>.FailureResult($"Error retrieving services: {ex.Message}");
        }
    }

    public async Task<ServiceResult<AppointmentResponseDto>> CreateAppointmentAsync(CreateAppointmentDto dto, Guid userId)
    {
        try
        {
            // Validate service exists
            var service = await _unitOfWork.Services.GetByIdAsync(dto.ServiceId);
            if (service == null)
                return ServiceResult<AppointmentResponseDto>.FailureResult("Service not found");

            // Validate pet belongs to user if provided
            if (dto.PetId.HasValue)
            {
                var pet = await _unitOfWork.Pets.GetByIdAsync(dto.PetId.Value);
                if (pet == null || pet.UserId != userId)
                    return ServiceResult<AppointmentResponseDto>.FailureResult("Pet not found or does not belong to you");
            }

            var appointment = new Appointment
            {
                UserId = userId,
                PetId = dto.PetId,
                ServiceId = dto.ServiceId,
                AppointmentType = dto.AppointmentType,
                AppointmentStatus = "pending",
                BranchId = dto.BranchId,
                AppointmentDate = DateTime.SpecifyKind(dto.AppointmentDate, DateTimeKind.Utc),
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                ServiceAddress = dto.ServiceAddress,
                Notes = dto.Notes
            };

            await _unitOfWork.Repository<Appointment>().AddAsync(appointment);

            // Record initial status history
            var history = new AppointmentStatusHistory
            {
                AppointmentId = appointment.Id,
                Status = "pending",
                Notes = "Lịch hẹn được tạo",
                UpdatedBy = userId
            };
            await _unitOfWork.Repository<AppointmentStatusHistory>().AddAsync(history);

            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.Appointments.GetAppointmentWithDetailsAsync(appointment.Id);
            return ServiceResult<AppointmentResponseDto>.SuccessResult(MapToResponseDto(created!), "Đặt lịch hẹn thành công");
        }
        catch (Exception ex)
        {
            return ServiceResult<AppointmentResponseDto>.FailureResult($"Error creating appointment: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<AppointmentResponseDto>>> GetUserAppointmentsAsync(Guid userId)
    {
        try
        {
            var appointments = await _unitOfWork.Appointments.GetAppointmentsByUserIdAsync(userId);
            var dtos = appointments.Select(MapToResponseDto);
            return ServiceResult<IEnumerable<AppointmentResponseDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<AppointmentResponseDto>>.FailureResult($"Error retrieving appointments: {ex.Message}");
        }
    }

    public async Task<ServiceResult<AppointmentResponseDto>> GetAppointmentByIdAsync(Guid appointmentId, Guid userId, string userRole)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetAppointmentWithDetailsAsync(appointmentId);
            if (appointment == null)
                return ServiceResult<AppointmentResponseDto>.FailureResult("Appointment not found");

            // Doctors and Admins can see any appointment; customers only see their own
            bool isPrivileged = userRole.Equals("Doctor", StringComparison.OrdinalIgnoreCase)
                             || userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            if (!isPrivileged && appointment.UserId != userId)
                return ServiceResult<AppointmentResponseDto>.FailureResult("You do not have permission to view this appointment");

            return ServiceResult<AppointmentResponseDto>.SuccessResult(MapToResponseDto(appointment));
        }
        catch (Exception ex)
        {
            return ServiceResult<AppointmentResponseDto>.FailureResult($"Error retrieving appointment: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> CancelAppointmentAsync(Guid appointmentId, Guid userId, string? cancellationReason)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null)
                return ServiceResult<bool>.FailureResult("Appointment not found");

            if (appointment.UserId != userId)
                return ServiceResult<bool>.FailureResult("You do not have permission to cancel this appointment");

            if (appointment.AppointmentStatus == "completed")
                return ServiceResult<bool>.FailureResult("Cannot cancel a completed appointment");

            if (appointment.AppointmentStatus == "cancelled")
                return ServiceResult<bool>.FailureResult("Appointment is already cancelled");

            appointment.AppointmentStatus = "cancelled";
            appointment.CancellationReason = cancellationReason;
            await _unitOfWork.Repository<Appointment>().UpdateAsync(appointment);

            var history = new AppointmentStatusHistory
            {
                AppointmentId = appointment.Id,
                Status = "cancelled",
                Notes = cancellationReason ?? "Khách hàng huỷ lịch",
                UpdatedBy = userId
            };
            await _unitOfWork.Repository<AppointmentStatusHistory>().AddAsync(history);

            await _unitOfWork.SaveChangesAsync();
            return ServiceResult<bool>.SuccessResult(true, "Lịch hẹn đã được huỷ");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error cancelling appointment: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<AppointmentResponseDto>>> GetAllAppointmentsAsync(string? status, DateTime? date)
    {
        try
        {
            var appointments = await _unitOfWork.Appointments.GetAllWithDetailsAsync(status, date);
            var dtos = appointments.Select(MapToResponseDto);
            return ServiceResult<IEnumerable<AppointmentResponseDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<AppointmentResponseDto>>.FailureResult($"Error retrieving appointments: {ex.Message}");
        }
    }

    public async Task<ServiceResult<AppointmentResponseDto>> UpdateAppointmentStatusAsync(Guid appointmentId, UpdateAppointmentStatusDto dto, Guid doctorId)
    {
        try
        {
            var validStatuses = new[] { "pending", "confirmed", "in-progress", "completed", "cancelled" };
            if (!validStatuses.Contains(dto.Status))
                return ServiceResult<AppointmentResponseDto>.FailureResult($"Invalid status. Valid values: {string.Join(", ", validStatuses)}");

            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            if (appointment == null)
                return ServiceResult<AppointmentResponseDto>.FailureResult("Appointment not found");

            // Fix tất cả DateTime trong appointment về UTC
            appointment.AppointmentDate = DateTime.SpecifyKind(appointment.AppointmentDate, DateTimeKind.Utc);
            appointment.CreatedAt = DateTime.SpecifyKind(appointment.CreatedAt, DateTimeKind.Utc);
            appointment.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

            appointment.AppointmentStatus = dto.Status;
            appointment.AssignedStaffId = doctorId;

            if (!string.IsNullOrWhiteSpace(dto.MedicalNotes))
                appointment.Notes = dto.MedicalNotes;

            if (dto.Status == "cancelled" && !string.IsNullOrWhiteSpace(dto.CancellationReason))
                appointment.CancellationReason = dto.CancellationReason;

            await _unitOfWork.Repository<Appointment>().UpdateAsync(appointment);

            var history = new AppointmentStatusHistory
            {
                AppointmentId = appointment.Id,
                Status = dto.Status,
                Notes = dto.MedicalNotes ?? dto.CancellationReason,
                UpdatedBy = doctorId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc) // fix luôn history
            };
            await _unitOfWork.Repository<AppointmentStatusHistory>().AddAsync(history);

            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.Appointments.GetAppointmentWithDetailsAsync(appointment.Id);
            return ServiceResult<AppointmentResponseDto>.SuccessResult(MapToResponseDto(updated!), "Cập nhật trạng thái thành công");
        }
        catch (Exception ex)
        {
            return ServiceResult<AppointmentResponseDto>.FailureResult($"Error: {ex.Message} | Inner: {ex.InnerException?.Message}");
        }
    }

    private static AppointmentResponseDto MapToResponseDto(Appointment a) => new()
    {
        Id = a.Id,
        UserId = a.UserId,
        UserName = a.User?.FullName ?? string.Empty,
        PetId = a.PetId,
        PetName = a.Pet?.PetName,
        ServiceId = a.ServiceId,
        ServiceName = a.Service?.ServiceName,
        ServicePrice = a.Service?.Price,
        AppointmentType = a.AppointmentType,
        AppointmentStatus = a.AppointmentStatus,
        BranchId = a.BranchId,
        BranchName = a.Branch?.BranchName,
        AssignedStaffId = a.AssignedStaffId,
        AssignedStaffName = a.AssignedStaff?.FullName,
        AppointmentDate = a.AppointmentDate,
        StartTime = a.StartTime,
        EndTime = a.EndTime,
        ServiceAddress = a.ServiceAddress,
        Notes = a.Notes,
        CancellationReason = a.CancellationReason,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };
}
