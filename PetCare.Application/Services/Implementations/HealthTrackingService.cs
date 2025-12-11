using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetCare.Application.Common;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Data;

namespace PetCare.Application.Services.Implementations;

public class HealthTrackingService : IHealthTrackingService
{
    private readonly PetCareDbContext _context;
    private readonly ILogger<HealthTrackingService> _logger;

    public HealthTrackingService(PetCareDbContext context, ILogger<HealthTrackingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Health Records
    public async Task<ServiceResult<HealthRecord>> CreateHealthRecordAsync(Guid petId, Guid userId, string recordType, string description, DateTime recordDate, Guid? veterinarianId = null)
    {
        try
        {
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == petId && p.UserId == userId);
            if (pet == null)
                return ServiceResult<HealthRecord>.FailureResult("Pet not found or you don't have permission");

            var record = new HealthRecord
            {
                PetId = petId,
                RecordDate = recordDate,
                Diagnosis = recordType,
                Treatment = description,
                RecordedBy = veterinarianId
            };

            _context.HealthRecords.Add(record);
            await _context.SaveChangesAsync();

            return ServiceResult<HealthRecord>.SuccessResult(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating health record for pet: {PetId}", petId);
            return ServiceResult<HealthRecord>.FailureResult($"Error creating health record: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<HealthRecord>>> GetPetHealthRecordsAsync(Guid petId, Guid userId)
    {
        try
        {
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == petId && p.UserId == userId);
            if (pet == null)
                return ServiceResult<List<HealthRecord>>.FailureResult("Pet not found or you don't have permission");

            var records = await _context.HealthRecords
                .Where(r => r.PetId == petId)
                .OrderByDescending(r => r.RecordDate)
                .ToListAsync();

            return ServiceResult<List<HealthRecord>>.SuccessResult(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health records for pet: {PetId}", petId);
            return ServiceResult<List<HealthRecord>>.FailureResult($"Error retrieving health records: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteHealthRecordAsync(Guid recordId, Guid userId)
    {
        try
        {
            var record = await _context.HealthRecords
                .Include(r => r.Pet)
                .FirstOrDefaultAsync(r => r.Id == recordId);

            if (record == null)
                return ServiceResult<bool>.FailureResult("Health record not found");

            if (record.Pet.UserId != userId)
                return ServiceResult<bool>.FailureResult("You don't have permission to delete this record");

            _context.HealthRecords.Remove(record);
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting health record: {RecordId}", recordId);
            return ServiceResult<bool>.FailureResult($"Error deleting health record: {ex.Message}");
        }
    }

    // Vaccinations
    public async Task<ServiceResult<Vaccination>> AddVaccinationAsync(Guid petId, Guid userId, string vaccineName, DateTime vaccinationDate, DateTime? nextDueDate = null, string? batchNumber = null)
    {
        try
        {
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == petId && p.UserId == userId);
            if (pet == null)
                return ServiceResult<Vaccination>.FailureResult("Pet not found or you don't have permission");

            var vaccination = new Vaccination
            {
                PetId = petId,
                VaccineName = vaccineName,
                VaccinationDate = vaccinationDate,
                NextDueDate = nextDueDate,
                BatchNumber = batchNumber
            };

            _context.Vaccinations.Add(vaccination);
            await _context.SaveChangesAsync();

            return ServiceResult<Vaccination>.SuccessResult(vaccination);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding vaccination for pet: {PetId}", petId);
            return ServiceResult<Vaccination>.FailureResult($"Error adding vaccination: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<Vaccination>>> GetPetVaccinationsAsync(Guid petId, Guid userId)
    {
        try
        {
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == petId && p.UserId == userId);
            if (pet == null)
                return ServiceResult<List<Vaccination>>.FailureResult("Pet not found or you don't have permission");

            var vaccinations = await _context.Vaccinations
                .Where(v => v.PetId == petId)
                .OrderByDescending(v => v.VaccinationDate)
                .ToListAsync();

            return ServiceResult<List<Vaccination>>.SuccessResult(vaccinations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vaccinations for pet: {PetId}", petId);
            return ServiceResult<List<Vaccination>>.FailureResult($"Error retrieving vaccinations: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<Vaccination>>> GetUpcomingVaccinationsAsync(Guid userId, int daysAhead = 30)
    {
        try
        {
            var userPetIds = await _context.Pets
                .Where(p => p.UserId == userId)
                .Select(p => p.Id)
                .ToListAsync();

            var upcomingDate = DateTime.UtcNow.AddDays(daysAhead);

            var vaccinations = await _context.Vaccinations
                .Include(v => v.Pet)
                .Where(v => userPetIds.Contains(v.PetId) &&
                           v.NextDueDate.HasValue &&
                           v.NextDueDate.Value <= upcomingDate &&
                           v.NextDueDate.Value >= DateTime.UtcNow)
                .OrderBy(v => v.NextDueDate)
                .ToListAsync();

            return ServiceResult<List<Vaccination>>.SuccessResult(vaccinations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upcoming vaccinations for user: {UserId}", userId);
            return ServiceResult<List<Vaccination>>.FailureResult($"Error retrieving upcoming vaccinations: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteVaccinationAsync(Guid vaccinationId, Guid userId)
    {
        try
        {
            var vaccination = await _context.Vaccinations
                .Include(v => v.Pet)
                .FirstOrDefaultAsync(v => v.Id == vaccinationId);

            if (vaccination == null)
                return ServiceResult<bool>.FailureResult("Vaccination record not found");

            if (vaccination.Pet.UserId != userId)
                return ServiceResult<bool>.FailureResult("You don't have permission to delete this vaccination");

            _context.Vaccinations.Remove(vaccination);
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vaccination: {VaccinationId}", vaccinationId);
            return ServiceResult<bool>.FailureResult($"Error deleting vaccination: {ex.Message}");
        }
    }

    // Health Reminders
    public async Task<ServiceResult<HealthReminder>> CreateReminderAsync(Guid petId, Guid userId, string reminderType, string title, DateTime reminderDate, string? notes = null)
    {
        try
        {
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == petId && p.UserId == userId);
            if (pet == null)
                return ServiceResult<HealthReminder>.FailureResult("Pet not found or you don't have permission");

            var reminder = new HealthReminder
            {
                PetId = petId,
                ReminderType = reminderType,
                ReminderTitle = title,
                ReminderDate = reminderDate,
                Notes = notes,
                IsCompleted = false
            };

            _context.HealthReminders.Add(reminder);
            await _context.SaveChangesAsync();

            return ServiceResult<HealthReminder>.SuccessResult(reminder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reminder for pet: {PetId}", petId);
            return ServiceResult<HealthReminder>.FailureResult($"Error creating reminder: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<HealthReminder>>> GetPetRemindersAsync(Guid petId, Guid userId, bool activeOnly = true)
    {
        try
        {
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == petId && p.UserId == userId);
            if (pet == null)
                return ServiceResult<List<HealthReminder>>.FailureResult("Pet not found or you don't have permission");

            var query = _context.HealthReminders.Where(r => r.PetId == petId);

            if (activeOnly)
                query = query.Where(r => !r.IsCompleted);

            var reminders = await query
                .OrderBy(r => r.ReminderDate)
                .ToListAsync();

            return ServiceResult<List<HealthReminder>>.SuccessResult(reminders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reminders for pet: {PetId}", petId);
            return ServiceResult<List<HealthReminder>>.FailureResult($"Error retrieving reminders: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<HealthReminder>>> GetUpcomingRemindersAsync(Guid userId, int daysAhead = 7)
    {
        try
        {
            var userPetIds = await _context.Pets
                .Where(p => p.UserId == userId)
                .Select(p => p.Id)
                .ToListAsync();

            var upcomingDate = DateTime.UtcNow.AddDays(daysAhead);

            var reminders = await _context.HealthReminders
                .Include(r => r.Pet)
                .Where(r => userPetIds.Contains(r.PetId) &&
                           !r.IsCompleted &&
                           r.ReminderDate <= upcomingDate &&
                           r.ReminderDate >= DateTime.UtcNow)
                .OrderBy(r => r.ReminderDate)
                .ToListAsync();

            return ServiceResult<List<HealthReminder>>.SuccessResult(reminders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upcoming reminders for user: {UserId}", userId);
            return ServiceResult<List<HealthReminder>>.FailureResult($"Error retrieving upcoming reminders: {ex.Message}");
        }
    }

    public async Task<ServiceResult<HealthReminder>> CompleteReminderAsync(Guid reminderId, Guid userId)
    {
        try
        {
            var reminder = await _context.HealthReminders
                .Include(r => r.Pet)
                .FirstOrDefaultAsync(r => r.Id == reminderId);

            if (reminder == null)
                return ServiceResult<HealthReminder>.FailureResult("Reminder not found");

            if (reminder.Pet.UserId != userId)
                return ServiceResult<HealthReminder>.FailureResult("You don't have permission to complete this reminder");

            reminder.IsCompleted = true;
            await _context.SaveChangesAsync();

            return ServiceResult<HealthReminder>.SuccessResult(reminder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing reminder: {ReminderId}", reminderId);
            return ServiceResult<HealthReminder>.FailureResult($"Error completing reminder: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteReminderAsync(Guid reminderId, Guid userId)
    {
        try
        {
            var reminder = await _context.HealthReminders
                .Include(r => r.Pet)
                .FirstOrDefaultAsync(r => r.Id == reminderId);

            if (reminder == null)
                return ServiceResult<bool>.FailureResult("Reminder not found");

            if (reminder.Pet.UserId != userId)
                return ServiceResult<bool>.FailureResult("You don't have permission to delete this reminder");

            _context.HealthReminders.Remove(reminder);
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting reminder: {ReminderId}", reminderId);
            return ServiceResult<bool>.FailureResult($"Error deleting reminder: {ex.Message}");
        }
    }
}



