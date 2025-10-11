using Microsoft.EntityFrameworkCore;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Data;
using PetCare.Infrastructure.Repositories.Interfaces;

namespace PetCare.Infrastructure.Repositories.Implementations;

public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(PetCareDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(a => a.Pet)
            .Include(a => a.Service)
            .Include(a => a.Branch)
            .Include(a => a.AssignedStaff)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(DateTime date)
    {
        return await _dbSet
            .Include(a => a.User)
            .Include(a => a.Pet)
            .Include(a => a.Service)
            .Include(a => a.AssignedStaff)
            .Where(a => a.AppointmentDate.Date == date.Date)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<Appointment?> GetAppointmentWithDetailsAsync(Guid appointmentId)
    {
        return await _dbSet
            .Include(a => a.User)
            .Include(a => a.Pet)
            .Include(a => a.Service)
            .Include(a => a.Branch)
            .Include(a => a.AssignedStaff)
            .Include(a => a.StatusHistory)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByStaffAsync(Guid staffId, DateTime date)
    {
        return await _dbSet
            .Include(a => a.User)
            .Include(a => a.Pet)
            .Include(a => a.Service)
            .Where(a => a.AssignedStaffId == staffId && a.AppointmentDate.Date == date.Date)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }
}
