using PetCare.Application.Common;
using PetCare.Application.DTOs.Appointment;
using PetCare.Domain.Entities;

namespace PetCare.Application.Services.Interfaces;

public interface IAppointmentService
{
    Task<ServiceResult<Appointment>> CreateAppointmentAsync(CreateAppointmentDto dto, Guid userId);
    Task<ServiceResult<Appointment>> UpdateAppointmentAsync(Guid id, UpdateAppointmentDto dto, Guid userId);
    Task<ServiceResult<Appointment>> CancelAppointmentAsync(Guid id, string cancellationReason, Guid userId);
    Task<ServiceResult<Appointment>> GetAppointmentByIdAsync(Guid id, Guid userId);
    Task<ServiceResult<PagedResult<Appointment>>> GetUserAppointmentsAsync(Guid userId, int page = 1, int pageSize = 10);
    Task<ServiceResult<PagedResult<Appointment>>> GetAllAppointmentsAsync(int page = 1, int pageSize = 10);
    Task<ServiceResult<Appointment>> AssignStaffAsync(Guid appointmentId, Guid staffId);
    Task<ServiceResult<Appointment>> UpdateStatusAsync(Guid appointmentId, string status, Guid userId);
    Task<ServiceResult<List<Appointment>>> GetAppointmentsByDateAsync(DateTime date);
    Task<ServiceResult<List<Appointment>>> GetStaffAppointmentsAsync(Guid staffId, DateTime? date = null);
}
