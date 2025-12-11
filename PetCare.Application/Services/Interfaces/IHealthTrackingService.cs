using PetCare.Application.Common;
using PetCare.Domain.Entities;

namespace PetCare.Application.Services.Interfaces;

public interface IHealthTrackingService
{
    // Health Records
    Task<ServiceResult<HealthRecord>> CreateHealthRecordAsync(Guid petId, Guid userId, string recordType, string description, DateTime recordDate, Guid? veterinarianId = null);
    Task<ServiceResult<List<HealthRecord>>> GetPetHealthRecordsAsync(Guid petId, Guid userId);
    Task<ServiceResult<bool>> DeleteHealthRecordAsync(Guid recordId, Guid userId);

    // Vaccinations
    Task<ServiceResult<Vaccination>> AddVaccinationAsync(Guid petId, Guid userId, string vaccineName, DateTime vaccinationDate, DateTime? nextDueDate = null, string? batchNumber = null);
    Task<ServiceResult<List<Vaccination>>> GetPetVaccinationsAsync(Guid petId, Guid userId);
    Task<ServiceResult<List<Vaccination>>> GetUpcomingVaccinationsAsync(Guid userId, int daysAhead = 30);
    Task<ServiceResult<bool>> DeleteVaccinationAsync(Guid vaccinationId, Guid userId);

    // Health Reminders
    Task<ServiceResult<HealthReminder>> CreateReminderAsync(Guid petId, Guid userId, string reminderType, string title, DateTime reminderDate, string? notes = null);
    Task<ServiceResult<List<HealthReminder>>> GetPetRemindersAsync(Guid petId, Guid userId, bool activeOnly = true);
    Task<ServiceResult<List<HealthReminder>>> GetUpcomingRemindersAsync(Guid userId, int daysAhead = 7);
    Task<ServiceResult<HealthReminder>> CompleteReminderAsync(Guid reminderId, Guid userId);
    Task<ServiceResult<bool>> DeleteReminderAsync(Guid reminderId, Guid userId);
}
