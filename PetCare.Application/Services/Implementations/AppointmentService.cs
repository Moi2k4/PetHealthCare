using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetCare.Application.Common;
using PetCare.Application.DTOs.Appointment;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Data;

namespace PetCare.Application.Services.Implementations;

public class AppointmentService : IAppointmentService
{
    private readonly PetCareDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        PetCareDbContext context,
        IMapper mapper,
        ILogger<AppointmentService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ServiceResult<Appointment>> CreateAppointmentAsync(CreateAppointmentDto dto, Guid userId)
    {
        try
        {
            // Validate user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return ServiceResult<Appointment>.FailureResult("User not found.");
            }

            // Validate service exists
            var service = await _context.Services.FindAsync(dto.ServiceId);
            if (service == null || !service.IsActive)
            {
                return ServiceResult<Appointment>.FailureResult("Service not found or inactive.");
            }

            // Validate pet if provided
            if (dto.PetId.HasValue)
            {
                var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == dto.PetId && p.UserId == userId);
                if (pet == null)
                {
                    return ServiceResult<Appointment>.FailureResult("Pet not found or does not belong to user.");
                }
            }

            // Validate branch if provided
            if (dto.BranchId.HasValue)
            {
                var branchExists = await _context.Branches.AnyAsync(b => b.Id == dto.BranchId && b.IsActive);
                if (!branchExists)
                {
                    return ServiceResult<Appointment>.FailureResult("Branch not found or inactive.");
                }
            }

            // Check for scheduling conflicts
            var hasConflict = await _context.Appointments
                .AnyAsync(a => 
                    a.AppointmentDate.Date == dto.AppointmentDate.Date &&
                    a.AppointmentStatus != "cancelled" &&
                    a.BranchId == dto.BranchId &&
                    ((dto.StartTime >= a.StartTime && dto.StartTime < a.EndTime) ||
                     (dto.EndTime > a.StartTime && dto.EndTime <= a.EndTime) ||
                     (dto.StartTime <= a.StartTime && dto.EndTime >= a.EndTime)));

            if (hasConflict)
            {
                return ServiceResult<Appointment>.FailureResult("Time slot is already booked. Please choose another time.");
            }

            var appointment = new Appointment
            {
                UserId = userId,
                PetId = dto.PetId,
                ServiceId = dto.ServiceId,
                AppointmentType = dto.AppointmentType,
                AppointmentStatus = "pending",
                BranchId = dto.BranchId,
                AppointmentDate = dto.AppointmentDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                ServiceAddress = dto.ServiceAddress,
                Notes = dto.Notes
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Add status history
            var statusHistory = new AppointmentStatusHistory
            {
                AppointmentId = appointment.Id,
                Status = "pending",
                Notes = "Appointment created"
            };
            _context.AppointmentStatusHistories.Add(statusHistory);
            await _context.SaveChangesAsync();

            // Load navigation properties
            await _context.Entry(appointment)
                .Reference(a => a.Service)
                .LoadAsync();
            await _context.Entry(appointment)
                .Reference(a => a.Pet)
                .LoadAsync();

            _logger.LogInformation("Appointment created successfully: {AppointmentId}", appointment.Id);
            return ServiceResult<Appointment>.SuccessResult(appointment, "Appointment created successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            return ServiceResult<Appointment>.FailureResult("Failed to create appointment.");
        }
    }

    public async Task<ServiceResult<Appointment>> UpdateAppointmentAsync(Guid id, UpdateAppointmentDto dto, Guid userId)
    {
        try
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (appointment == null)
            {
                return ServiceResult<Appointment>.FailureResult("Appointment not found.");
            }

            if (appointment.AppointmentStatus == "completed" || appointment.AppointmentStatus == "cancelled")
            {
                return ServiceResult<Appointment>.FailureResult("Cannot update completed or cancelled appointment.");
            }

            // Update fields if provided
            if (dto.PetId.HasValue) appointment.PetId = dto.PetId;
            if (dto.ServiceId.HasValue) appointment.ServiceId = dto.ServiceId;
            if (!string.IsNullOrEmpty(dto.AppointmentType)) appointment.AppointmentType = dto.AppointmentType;
            if (dto.BranchId.HasValue) appointment.BranchId = dto.BranchId;
            if (dto.AppointmentDate.HasValue) appointment.AppointmentDate = dto.AppointmentDate.Value;
            if (dto.StartTime.HasValue) appointment.StartTime = dto.StartTime.Value;
            if (dto.EndTime.HasValue) appointment.EndTime = dto.EndTime.Value;
            if (dto.ServiceAddress != null) appointment.ServiceAddress = dto.ServiceAddress;
            if (dto.Notes != null) appointment.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Appointment updated: {AppointmentId}", id);
            return ServiceResult<Appointment>.SuccessResult(appointment, "Appointment updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment {Id}", id);
            return ServiceResult<Appointment>.FailureResult("Failed to update appointment.");
        }
    }

    public async Task<ServiceResult<Appointment>> CancelAppointmentAsync(Guid id, string cancellationReason, Guid userId)
    {
        try
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (appointment == null)
            {
                return ServiceResult<Appointment>.FailureResult("Appointment not found.");
            }

            if (appointment.AppointmentStatus == "cancelled")
            {
                return ServiceResult<Appointment>.FailureResult("Appointment is already cancelled.");
            }

            appointment.AppointmentStatus = "cancelled";
            appointment.CancellationReason = cancellationReason;

            // Add status history
            var statusHistory = new AppointmentStatusHistory
            {
                AppointmentId = appointment.Id,
                Status = "cancelled",
                Notes = cancellationReason
            };
            _context.AppointmentStatusHistories.Add(statusHistory);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Appointment cancelled: {AppointmentId}", id);
            return ServiceResult<Appointment>.SuccessResult(appointment, "Appointment cancelled successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling appointment {Id}", id);
            return ServiceResult<Appointment>.FailureResult("Failed to cancel appointment.");
        }
    }

    public async Task<ServiceResult<Appointment>> GetAppointmentByIdAsync(Guid id, Guid userId)
    {
        try
        {
            var appointment = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Pet)
                .Include(a => a.Branch)
                .Include(a => a.AssignedStaff)
                .Include(a => a.StatusHistory)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (appointment == null)
            {
                return ServiceResult<Appointment>.FailureResult("Appointment not found.");
            }

            return ServiceResult<Appointment>.SuccessResult(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointment {Id}", id);
            return ServiceResult<Appointment>.FailureResult("Failed to retrieve appointment.");
        }
    }

    public async Task<ServiceResult<PagedResult<Appointment>>> GetUserAppointmentsAsync(Guid userId, int page = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Pet)
                .Include(a => a.Branch)
                .Include(a => a.AssignedStaff)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AppointmentDate);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagedResult = new PagedResult<Appointment>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<Appointment>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user appointments for user {UserId}", userId);
            return ServiceResult<PagedResult<Appointment>>.FailureResult("Failed to retrieve appointments.");
        }
    }

    public async Task<ServiceResult<PagedResult<Appointment>>> GetAllAppointmentsAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Pet)
                .Include(a => a.Branch)
                .Include(a => a.User)
                .Include(a => a.AssignedStaff)
                .OrderByDescending(a => a.AppointmentDate);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagedResult = new PagedResult<Appointment>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<Appointment>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all appointments");
            return ServiceResult<PagedResult<Appointment>>.FailureResult("Failed to retrieve appointments.");
        }
    }

    public async Task<ServiceResult<Appointment>> AssignStaffAsync(Guid appointmentId, Guid staffId)
    {
        try
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
            {
                return ServiceResult<Appointment>.FailureResult("Appointment not found.");
            }

            var staff = await _context.Users.FindAsync(staffId);
            if (staff == null)
            {
                return ServiceResult<Appointment>.FailureResult("Staff not found.");
            }

            appointment.AssignedStaffId = staffId;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Staff {StaffId} assigned to appointment {AppointmentId}", staffId, appointmentId);
            return ServiceResult<Appointment>.SuccessResult(appointment, "Staff assigned successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning staff to appointment {AppointmentId}", appointmentId);
            return ServiceResult<Appointment>.FailureResult("Failed to assign staff.");
        }
    }

    public async Task<ServiceResult<Appointment>> UpdateStatusAsync(Guid appointmentId, string status, Guid userId)
    {
        try
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
            {
                return ServiceResult<Appointment>.FailureResult("Appointment not found.");
            }

            var validStatuses = new[] { "pending", "confirmed", "in-progress", "completed", "cancelled" };
            if (!validStatuses.Contains(status.ToLower()))
            {
                return ServiceResult<Appointment>.FailureResult("Invalid status.");
            }

            appointment.AppointmentStatus = status.ToLower();

            var statusHistory = new AppointmentStatusHistory
            {
                AppointmentId = appointmentId,
                Status = status.ToLower(),
                UpdatedBy = userId
            };
            _context.AppointmentStatusHistories.Add(statusHistory);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Appointment {AppointmentId} status updated to {Status}", appointmentId, status);
            return ServiceResult<Appointment>.SuccessResult(appointment, "Status updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment status");
            return ServiceResult<Appointment>.FailureResult("Failed to update status.");
        }
    }

    public async Task<ServiceResult<List<Appointment>>> GetAppointmentsByDateAsync(DateTime date)
    {
        try
        {
            var appointments = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Pet)
                .Include(a => a.User)
                .Include(a => a.Branch)
                .Include(a => a.AssignedStaff)
                .Where(a => a.AppointmentDate.Date == date.Date)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            return ServiceResult<List<Appointment>>.SuccessResult(appointments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointments by date {Date}", date);
            return ServiceResult<List<Appointment>>.FailureResult("Failed to retrieve appointments.");
        }
    }

    public async Task<ServiceResult<List<Appointment>>> GetStaffAppointmentsAsync(Guid staffId, DateTime? date = null)
    {
        try
        {
            var query = _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Pet)
                .Include(a => a.User)
                .Include(a => a.Branch)
                .Where(a => a.AssignedStaffId == staffId);

            if (date.HasValue)
            {
                query = query.Where(a => a.AppointmentDate.Date == date.Value.Date);
            }

            var appointments = await query
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            return ServiceResult<List<Appointment>>.SuccessResult(appointments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting staff appointments for {StaffId}", staffId);
            return ServiceResult<List<Appointment>>.FailureResult("Failed to retrieve appointments.");
        }
    }
}

