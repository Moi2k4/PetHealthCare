using PetCare.Application.Common;
using PetCare.Application.DTOs.Appointment;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Domain.Interfaces;
using PetCare.Infrastructure.Repositories.Interfaces;

namespace PetCare.Application.Services.Implementations;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public AppointmentService(IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;

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
            try
            {
                var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
                if (user != null)
                {
                    await _emailService.SendEmailAsync(
                        user.Email,
                        "Đặt lịch hẹn thành công - PetCare",
                        BuildBookingConfirmationEmailBody(user.FullName, appointment.AppointmentDate, appointment.StartTime, service.ServiceName)
                    );
                }
            }
            catch
            {
                // Bỏ qua lỗi email
            }

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

            // Fix DateTime về UTC
            appointment.AppointmentDate = DateTime.SpecifyKind(appointment.AppointmentDate, DateTimeKind.Utc);
            appointment.CreatedAt = DateTime.SpecifyKind(appointment.CreatedAt, DateTimeKind.Utc);
            appointment.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

            appointment.AppointmentStatus = "cancelled";
            appointment.CancellationReason = cancellationReason;
            await _unitOfWork.Repository<Appointment>().UpdateAsync(appointment);

            var history = new AppointmentStatusHistory
            {
                AppointmentId = appointment.Id,
                Status = "cancelled",
                Notes = cancellationReason ?? "Khách hàng huỷ lịch",
                UpdatedBy = userId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };
            await _unitOfWork.Repository<AppointmentStatusHistory>().AddAsync(history);

            await _unitOfWork.SaveChangesAsync();

            // Gửi email thông báo huỷ
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(appointment.UserId);
                if (user != null)
                {
                    await _emailService.SendEmailAsync(
                         user.Email,
                         "Xác nhận huỷ lịch hẹn - PetCare",
                         BuildCancelledEmailBody(user.FullName, appointment.AppointmentDate, cancellationReason)
                     );
                }
            }
            catch
            {
                // Bỏ qua lỗi email, không ảnh hưởng kết quả chính
            }

            return ServiceResult<bool>.SuccessResult(true, "Lịch hẹn đã được huỷ");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error cancelling appointment: {ex.Message} | Inner: {ex.InnerException?.Message}");
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

            // Fix DateTime về UTC
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
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };
            await _unitOfWork.Repository<AppointmentStatusHistory>().AddAsync(history);

            await _unitOfWork.SaveChangesAsync();

            // Gửi email thông báo
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(appointment.UserId);
                if (user != null)
                {
                    if (dto.Status == "confirmed")
                    {
                        await _emailService.SendEmailAsync(
                            user.Email,
                            "Lịch hẹn đã được xác nhận - PetCare",
                            BuildConfirmedEmailBody(user.FullName, appointment.AppointmentDate, appointment.StartTime, dto.MedicalNotes)
                        );
                    }
                    else if (dto.Status == "cancelled")
                    {
                        await _emailService.SendEmailAsync(
                            user.Email,
                            "Lịch hẹn đã bị huỷ - PetCare",
                            BuildCancelledEmailBody(user.FullName, appointment.AppointmentDate, dto.CancellationReason)
                        );
                    }
                    else if (dto.Status == "completed")
                    {
                        await _emailService.SendEmailAsync(
                            user.Email,
                            "Lịch hẹn đã hoàn thành - PetCare",
                            BuildCompletedEmailBody(user.FullName, appointment.AppointmentDate, dto.MedicalNotes)
                        );
                    }

                }
            }
            catch
            {
                // Bỏ qua lỗi email, không ảnh hưởng kết quả chính
            }

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
    private string BuildConfirmedEmailBody(string fullName, DateTime appointmentDate, TimeSpan? startTime, string? notes) => $"""
    <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
        <div style="background-color: #4f9d69; padding: 30px; text-align: center; border-radius: 8px 8px 0 0;">
            <h1 style="color: white; margin: 0;">Lịch hẹn đã được xác nhận ✅</h1>
        </div>
        <div style="padding: 30px; background-color: #f9f9f9; border-radius: 0 0 8px 8px;">
            <p style="font-size: 16px;">Xin chào <strong>{fullName}</strong>,</p>
            <p style="font-size: 16px;">Lịch hẹn của bạn đã được <strong>xác nhận</strong> thành công.</p>
            <div style="background-color: #fff; border: 1px solid #e0e0e0; border-radius: 6px; padding: 20px; margin: 20px 0;">
                <p style="margin: 0; font-size: 15px;"><strong>Ngày hẹn:</strong> {appointmentDate:dd/MM/yyyy}</p>
                <p style="margin: 8px 0 0; font-size: 15px;"><strong>Giờ bắt đầu:</strong> {startTime}</p>
                {(!string.IsNullOrWhiteSpace(notes) ? $"<p style=\"margin: 8px 0 0; font-size: 15px;\"><strong>Ghi chú:</strong> {notes}</p>" : "")}
            </div>
            <div style="text-align: center; margin: 30px 0;">
                <a href="https://pettsuba.live" style="background-color: #4f9d69; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; font-size: 16px;">Xem lịch hẹn</a>
            </div>
            <p style="color: #888; font-size: 13px; text-align: center;">Cảm ơn bạn đã sử dụng dịch vụ PetCare! 🐾</p>
        </div>
    </div>
    """;

    private string BuildCancelledEmailBody(string fullName, DateTime appointmentDate, string? reason) => $"""
    <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
        <div style="background-color: #dc2626; padding: 30px; text-align: center; border-radius: 8px 8px 0 0;">
            <h1 style="color: white; margin: 0;">Lịch hẹn đã bị huỷ ❌</h1>
        </div>
        <div style="padding: 30px; background-color: #f9f9f9; border-radius: 0 0 8px 8px;">
            <p style="font-size: 16px;">Xin chào <strong>{fullName}</strong>,</p>
            <p style="font-size: 16px;">Lịch hẹn của bạn đã bị <strong>huỷ</strong>.</p>
            <div style="background-color: #fff; border: 1px solid #e0e0e0; border-radius: 6px; padding: 20px; margin: 20px 0;">
                <p style="margin: 0; font-size: 15px;"><strong>Ngày hẹn:</strong> {appointmentDate:dd/MM/yyyy}</p>
                <p style="margin: 8px 0 0; font-size: 15px;"><strong>Lý do:</strong> {reason ?? "Không có lý do"}</p>
            </div>
            <div style="text-align: center; margin: 30px 0;">
                <a href="https://pettsuba.live" style="background-color: #4f9d69; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; font-size: 16px;">Đặt lịch lại</a>
            </div>
            <p style="color: #888; font-size: 13px; text-align: center;">Vui lòng liên hệ PetCare nếu bạn có thắc mắc.</p>
        </div>
    </div>
    """;

    private string BuildCompletedEmailBody(string fullName, DateTime appointmentDate, string? notes) => $"""
    <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
        <div style="background-color: #2563eb; padding: 30px; text-align: center; border-radius: 8px 8px 0 0;">
            <h1 style="color: white; margin: 0;">Lịch hẹn đã hoàn thành 🎉</h1>
        </div>
        <div style="padding: 30px; background-color: #f9f9f9; border-radius: 0 0 8px 8px;">
            <p style="font-size: 16px;">Xin chào <strong>{fullName}</strong>,</p>
            <p style="font-size: 16px;">Lịch hẹn của bạn đã <strong>hoàn thành</strong> thành công.</p>
            <div style="background-color: #fff; border: 1px solid #e0e0e0; border-radius: 6px; padding: 20px; margin: 20px 0;">
                <p style="margin: 0; font-size: 15px;"><strong>Ngày hẹn:</strong> {appointmentDate:dd/MM/yyyy}</p>
                {(!string.IsNullOrWhiteSpace(notes) ? $"<p style=\"margin: 8px 0 0; font-size: 15px;\"><strong>Ghi chú bác sĩ:</strong> {notes}</p>" : "")}
            </div>
            <div style="text-align: center; margin: 30px 0;">
                <a href="https://pettsuba.live" style="background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; font-size: 16px;">Xem chi tiết</a>
            </div>
            <p style="color: #888; font-size: 13px; text-align: center;">Cảm ơn bạn đã sử dụng dịch vụ PetCare! 🐾</p>
        </div>
    </div>
    """;
    private string BuildBookingConfirmationEmailBody(string fullName, DateTime appointmentDate, TimeSpan? startTime, string? serviceName) => $"""
    <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
        <div style="background-color: #4f9d69; padding: 30px; text-align: center; border-radius: 8px 8px 0 0;">
            <h1 style="color: white; margin: 0;">Đặt lịch hẹn thành công 🐾</h1>
        </div>
        <div style="padding: 30px; background-color: #f9f9f9; border-radius: 0 0 8px 8px;">
            <p style="font-size: 16px;">Xin chào <strong>{fullName}</strong>,</p>
            <p style="font-size: 16px;">Bạn đã đặt lịch hẹn thành công. Chúng tôi sẽ xác nhận lịch hẹn sớm nhất có thể.</p>
            <div style="background-color: #fff; border: 1px solid #e0e0e0; border-radius: 6px; padding: 20px; margin: 20px 0;">
                <p style="margin: 0; font-size: 15px;"><strong>Dịch vụ:</strong> {serviceName ?? "N/A"}</p>
                <p style="margin: 8px 0 0; font-size: 15px;"><strong>Ngày hẹn:</strong> {appointmentDate:dd/MM/yyyy}</p>
                <p style="margin: 8px 0 0; font-size: 15px;"><strong>Giờ bắt đầu:</strong> {startTime}</p>
                <p style="margin: 8px 0 0; font-size: 15px;"><strong>Trạng thái:</strong> Chờ xác nhận</p>
            </div>
            <div style="text-align: center; margin: 30px 0;">
                <a href="https://pettsuba.live" style="background-color: #4f9d69; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; font-size: 16px;">Xem lịch hẹn</a>
            </div>
            <p style="color: #888; font-size: 13px; text-align: center;">Cảm ơn bạn đã sử dụng dịch vụ PetCare! 🐾</p>
        </div>
    </div>
    """;
}
