using PetCare.Domain.Entities;

namespace PetCare.Infrastructure.Repositories.Interfaces;

public interface IAppointmentRepository : IGenericRepository<Appointment>
{
    Task<IEnumerable<Appointment>> GetAppointmentsByUserIdAsync(Guid userId);
    Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(DateTime date);
    Task<Appointment?> GetAppointmentWithDetailsAsync(Guid appointmentId);
    Task<IEnumerable<Appointment>> GetAppointmentsByStaffAsync(Guid staffId, DateTime date);
}
