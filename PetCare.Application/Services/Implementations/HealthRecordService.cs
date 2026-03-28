using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PetCare.Application.Common;
using PetCare.Application.DTOs.Health;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Repositories.Interfaces;

namespace PetCare.Application.Services.Implementations;

public class HealthRecordService : IHealthRecordService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public HealthRecordService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<IEnumerable<HealthRecordDto>>> GetByPetAsync(Guid petId, Guid requestingUserId)
    {
        try
        {
            var pet = await _unitOfWork.Pets.GetByIdAsync(petId);
            if (pet == null)
                return ServiceResult<IEnumerable<HealthRecordDto>>.FailureResult("Pet not found");

            // Only the pet owner, staff, or admin can view records
            if (pet.UserId != requestingUserId)
            {
                var user = await _unitOfWork.Users.GetUserWithRoleAsync(requestingUserId);
                var role = user?.Role?.RoleName?.ToLower();
                if (role != "admin" && role != "staff")
                    return ServiceResult<IEnumerable<HealthRecordDto>>.FailureResult("You don't have permission to view this pet's records");
            }

            var records = await _unitOfWork.Repository<HealthRecord>()
                .QueryWithIncludes(r => r.Pet, r => r.RecordedByUser!)
                .Where(r => r.PetId == petId)
                .OrderByDescending(r => r.RecordDate)
                .ToListAsync();

            var dtos = _mapper.Map<IEnumerable<HealthRecordDto>>(records);
            return ServiceResult<IEnumerable<HealthRecordDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<HealthRecordDto>>.FailureResult($"Error retrieving health records: {ex.Message}");
        }
    }

    public async Task<ServiceResult<HealthRecordDto>> GetByIdAsync(Guid recordId, Guid requestingUserId)
    {
        try
        {
            var record = await _unitOfWork.Repository<HealthRecord>()
                .QueryWithIncludes(r => r.Pet, r => r.RecordedByUser!)
                .FirstOrDefaultAsync(r => r.Id == recordId);

            if (record == null)
                return ServiceResult<HealthRecordDto>.FailureResult("Health record not found");

            if (record.Pet.UserId != requestingUserId)
            {
                var user = await _unitOfWork.Users.GetUserWithRoleAsync(requestingUserId);
                var role = user?.Role?.RoleName?.ToLower();
                if (role != "admin" && role != "staff")
                    return ServiceResult<HealthRecordDto>.FailureResult("You don't have permission to view this record");
            }

            var dto = _mapper.Map<HealthRecordDto>(record);
            return ServiceResult<HealthRecordDto>.SuccessResult(dto);
        }
        catch (Exception ex)
        {
            return ServiceResult<HealthRecordDto>.FailureResult($"Error retrieving health record: {ex.Message}");
        }
    }

    public async Task<ServiceResult<HealthRecordDto>> CreateAsync(CreateHealthRecordDto dto, Guid recordedByUserId)
    {
        try
        {
            var pet = await _unitOfWork.Pets.GetByIdAsync(dto.PetId);
            if (pet == null)
                return ServiceResult<HealthRecordDto>.FailureResult("Pet not found");

            // Only pet owner, staff, or admin can create records
            if (pet.UserId != recordedByUserId)
            {
                var user = await _unitOfWork.Users.GetUserWithRoleAsync(recordedByUserId);
                var role = user?.Role?.RoleName?.ToLower();
                if (role != "admin" && role != "staff")
                    return ServiceResult<HealthRecordDto>.FailureResult("You don't have permission to add records for this pet");
            }

            var record = _mapper.Map<HealthRecord>(dto);
            record.RecordedBy = recordedByUserId;

            await _unitOfWork.Repository<HealthRecord>().AddAsync(record);
            await _unitOfWork.SaveChangesAsync();

            // Reload with includes for response
            var created = await _unitOfWork.Repository<HealthRecord>()
                .QueryWithIncludes(r => r.Pet, r => r.RecordedByUser!)
                .FirstOrDefaultAsync(r => r.Id == record.Id);

            var result = _mapper.Map<HealthRecordDto>(created);
            return ServiceResult<HealthRecordDto>.SuccessResult(result, "Health record created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<HealthRecordDto>.FailureResult($"Error creating health record: {ex.Message}");
        }
    }

    public async Task<ServiceResult<HealthRecordDto>> UpdateAsync(Guid recordId, UpdateHealthRecordDto dto, Guid requestingUserId)
    {
        try
        {
            var record = await _unitOfWork.Repository<HealthRecord>()
                .QueryWithIncludes(r => r.Pet, r => r.RecordedByUser!)
                .FirstOrDefaultAsync(r => r.Id == recordId);

            if (record == null)
                return ServiceResult<HealthRecordDto>.FailureResult("Health record not found");

            // Only pet owner, staff, or admin can update
            if (record.Pet.UserId != requestingUserId)
            {
                var user = await _unitOfWork.Users.GetUserWithRoleAsync(requestingUserId);
                var role = user?.Role?.RoleName?.ToLower();
                if (role != "admin" && role != "staff")
                    return ServiceResult<HealthRecordDto>.FailureResult("You don't have permission to update this record");
            }

            if (dto.RecordDate.HasValue) record.RecordDate = dto.RecordDate.Value;
            if (dto.Weight.HasValue) record.Weight = dto.Weight;
            if (dto.Height.HasValue) record.Height = dto.Height;
            if (dto.Temperature.HasValue) record.Temperature = dto.Temperature;
            if (dto.HeartRate.HasValue) record.HeartRate = dto.HeartRate;
            if (dto.Diagnosis != null) record.Diagnosis = dto.Diagnosis;
            if (dto.Treatment != null) record.Treatment = dto.Treatment;
            if (dto.Notes != null) record.Notes = dto.Notes;

            await _unitOfWork.Repository<HealthRecord>().UpdateAsync(record);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<HealthRecordDto>(record);
            return ServiceResult<HealthRecordDto>.SuccessResult(result, "Health record updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<HealthRecordDto>.FailureResult($"Error updating health record: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid recordId, Guid requestingUserId)
    {
        try
        {
            var record = await _unitOfWork.Repository<HealthRecord>()
                .QueryWithIncludes(r => r.Pet)
                .FirstOrDefaultAsync(r => r.Id == recordId);

            if (record == null)
                return ServiceResult<bool>.FailureResult("Health record not found");

            // Only pet owner, staff, or admin can delete
            if (record.Pet.UserId != requestingUserId)
            {
                var user = await _unitOfWork.Users.GetUserWithRoleAsync(requestingUserId);
                var role = user?.Role?.RoleName?.ToLower();
                if (role != "admin" && role != "staff")
                    return ServiceResult<bool>.FailureResult("You don't have permission to delete this record");
            }

            await _unitOfWork.Repository<HealthRecord>().DeleteAsync(record);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true, "Health record deleted successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error deleting health record: {ex.Message}");
        }
    }

    public async Task<ServiceResult<VaccinationDto>> AddVaccinationAsync(Guid petId, CreateVaccinationDto dto, Guid requestingUserId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.VaccineName))
                return ServiceResult<VaccinationDto>.FailureResult("Vaccine name is required");

            var pet = await _unitOfWork.Pets.GetByIdAsync(petId);
            if (pet == null)
                return ServiceResult<VaccinationDto>.FailureResult("Pet not found");

            if (pet.UserId != requestingUserId)
            {
                var user = await _unitOfWork.Users.GetUserWithRoleAsync(requestingUserId);
                var role = user?.Role?.RoleName?.ToLowerInvariant();
                if (role != "admin" && role != "staff")
                    return ServiceResult<VaccinationDto>.FailureResult("You don't have permission to add vaccination for this pet");
            }

            var entity = new Vaccination
            {
                PetId = petId,
                VaccineName = dto.VaccineName.Trim(),
                VaccinationDate = dto.VaccinationDate ?? DateTime.UtcNow,
                NextDueDate = dto.NextDueDate,
                BatchNumber = dto.BatchNumber,
                AdministeredBy = requestingUserId,
                Notes = dto.Notes
            };

            await _unitOfWork.Repository<Vaccination>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var result = new VaccinationDto
            {
                Id = entity.Id,
                PetId = entity.PetId,
                VaccineName = entity.VaccineName,
                VaccinationDate = entity.VaccinationDate,
                NextDueDate = entity.NextDueDate,
                BatchNumber = entity.BatchNumber,
                AdministeredBy = entity.AdministeredBy,
                Notes = entity.Notes,
                CreatedAt = entity.CreatedAt
            };

            return ServiceResult<VaccinationDto>.SuccessResult(result, "Vaccination recorded successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<VaccinationDto>.FailureResult($"Error recording vaccination: {ex.Message}");
        }
    }

    public async Task<ServiceResult<DogRoutineScheduleDto>> GetDogRoutineScheduleAsync(Guid petId, Guid requestingUserId)
    {
        try
        {
            var now = DateTime.UtcNow;
            var activeSubscription = await _unitOfWork.Repository<UserSubscription>()
                .QueryWithIncludes(s => s.SubscriptionPackage)
                .Where(s => s.UserId == requestingUserId
                    && s.IsActive
                    && s.Status == "Active"
                    && (s.EndDate == null || s.EndDate >= now))
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();

            if (activeSubscription == null)
                return ServiceResult<DogRoutineScheduleDto>.FailureResult("Dog routine is available for members only.");

            if (activeSubscription.SubscriptionPackage == null || !activeSubscription.SubscriptionPackage.HasHealthReminders)
                return ServiceResult<DogRoutineScheduleDto>.FailureResult("Your current membership does not include dog routine reminders.");

            var pet = await _unitOfWork.Repository<Pet>()
                .QueryWithIncludes(p => p.Species)
                .FirstOrDefaultAsync(p => p.Id == petId);

            if (pet == null)
                return ServiceResult<DogRoutineScheduleDto>.FailureResult("Pet not found");

            if (pet.UserId != requestingUserId)
            {
                var user = await _unitOfWork.Users.GetUserWithRoleAsync(requestingUserId);
                var role = user?.Role?.RoleName?.ToLowerInvariant();
                if (role != "admin" && role != "staff")
                    return ServiceResult<DogRoutineScheduleDto>.FailureResult("You don't have permission to view this pet's routine schedule");
            }

            var speciesName = pet.Species?.SpeciesName?.Trim().ToLowerInvariant() ?? string.Empty;
            var isDog = speciesName.Contains("dog")
                || speciesName.Contains("cho")
                || speciesName.Contains("chó")
                || speciesName.Contains("canine");

            var schedule = new DogRoutineScheduleDto
            {
                PetId = pet.Id,
                PetName = pet.PetName,
                IsDog = isDog,
                DateOfBirth = pet.DateOfBirth,
                Note = pet.DateOfBirth.HasValue
                    ? null
                    : "Date of birth is missing, so due dates are estimated from today."
            };

            if (!isDog)
            {
                schedule.Note = "Routine currently supports dogs only.";
                return ServiceResult<DogRoutineScheduleDto>.SuccessResult(schedule);
            }

            var vaccinations = await _unitOfWork.Repository<Vaccination>()
                .FindAsync(v => v.PetId == petId);

            var dewormingHistory = await _unitOfWork.Repository<HealthReminder>()
                .FindAsync(r => r.PetId == petId
                    && (EF.Functions.ILike(r.ReminderType, "%deworm%")
                        || EF.Functions.ILike(r.ReminderType, "%tay giun%")
                        || EF.Functions.ILike(r.ReminderTitle, "%deworm%")
                        || EF.Functions.ILike(r.ReminderTitle, "%tay giun%")
                        || EF.Functions.ILike(r.ReminderTitle, "%tẩy giun%")));

            var today = DateTime.UtcNow.Date;
            var baselineDate = pet.DateOfBirth?.Date ?? today;

            schedule.Vaccinations = BuildVaccinationRoutine(vaccinations, baselineDate, today);
            schedule.Deworming = BuildDewormingRoutine(dewormingHistory, baselineDate, today);

            return ServiceResult<DogRoutineScheduleDto>.SuccessResult(schedule);
        }
        catch (Exception ex)
        {
            return ServiceResult<DogRoutineScheduleDto>.FailureResult($"Error retrieving dog routine schedule: {ex.Message}");
        }
    }

    private static List<DogRoutineItemDto> BuildVaccinationRoutine(IEnumerable<Vaccination> vaccinations, DateTime baselineDate, DateTime today)
    {
        var vaccineList = vaccinations.OrderBy(v => v.VaccinationDate).ToList();

        var dhppKeywords = new[] { "dhpp", "dhlpp", "distemper", "parvo" };
        var rabiesKeywords = new[] { "rabies", "dai", "dại" };

        var dhppHistory = vaccineList.Where(v => ContainsAny(v.VaccineName, dhppKeywords)).ToList();
        var rabiesHistory = vaccineList.Where(v => ContainsAny(v.VaccineName, rabiesKeywords)).ToList();

        var items = new List<DogRoutineItemDto>();

        items.Add(BuildSeriesItem("Vaccination", "DHPP - First dose", baselineDate.AddDays(42), "One-time", dhppHistory.ElementAtOrDefault(0)?.VaccinationDate, today));
        items.Add(BuildSeriesItem("Vaccination", "DHPP - Booster 1", baselineDate.AddDays(56), "One-time", dhppHistory.ElementAtOrDefault(1)?.VaccinationDate, today));
        items.Add(BuildSeriesItem("Vaccination", "DHPP - Booster 2", baselineDate.AddDays(84), "One-time", dhppHistory.ElementAtOrDefault(2)?.VaccinationDate, today));
        items.Add(BuildSeriesItem("Vaccination", "Rabies", baselineDate.AddDays(112), "One-time", rabiesHistory.FirstOrDefault()?.VaccinationDate, today));

        var annualReference = rabiesHistory.OrderByDescending(v => v.VaccinationDate).FirstOrDefault()?.VaccinationDate;
        var annualDueDate = annualReference.HasValue ? annualReference.Value.Date.AddYears(1) : baselineDate.AddDays(112).AddYears(1);
        items.Add(new DogRoutineItemDto
        {
            Category = "Vaccination",
            ItemName = "Annual booster (core vaccines)",
            DueDate = annualDueDate,
            LastCompletedDate = annualReference,
            Frequency = "Every 12 months",
            Status = CalculateStatus(annualDueDate, annualReference, today),
            Source = annualReference.HasValue ? "vaccinations" : "estimated"
        });

        return items;
    }

    private static List<DogRoutineItemDto> BuildDewormingRoutine(IEnumerable<HealthReminder> reminders, DateTime baselineDate, DateTime today)
    {
        var history = reminders
            .Where(r => r.IsCompleted)
            .OrderByDescending(r => r.ReminderDate)
            .ToList();

        var ageInMonths = ((today.Year - baselineDate.Year) * 12) + today.Month - baselineDate.Month;
        if (today.Day < baselineDate.Day)
        {
            ageInMonths--;
        }

        var isPuppy = ageInMonths < 6;
        var intervalMonths = isPuppy ? 1 : 3;
        var frequency = isPuppy ? "Every month until 6 months old" : "Every 3 months";

        var lastCompleted = history.FirstOrDefault()?.ReminderDate.Date;
        var firstRecommended = baselineDate.AddDays(14);

        var dueDate = lastCompleted.HasValue
            ? lastCompleted.Value.AddMonths(intervalMonths)
            : (firstRecommended > today ? firstRecommended : today);

        return new List<DogRoutineItemDto>
        {
            new DogRoutineItemDto
            {
                Category = "Deworming",
                ItemName = "Routine deworming",
                DueDate = dueDate,
                LastCompletedDate = lastCompleted,
                Frequency = frequency,
                Status = CalculateStatus(dueDate, lastCompleted, today),
                Source = lastCompleted.HasValue ? "health_reminders" : "estimated"
            }
        };
    }

    private static DogRoutineItemDto BuildSeriesItem(
        string category,
        string itemName,
        DateTime dueDate,
        string frequency,
        DateTime? completedDate,
        DateTime today)
    {
        return new DogRoutineItemDto
        {
            Category = category,
            ItemName = itemName,
            DueDate = dueDate,
            LastCompletedDate = completedDate,
            Frequency = frequency,
            Status = CalculateStatus(dueDate, completedDate, today),
            Source = completedDate.HasValue ? "vaccinations" : "estimated"
        };
    }

    private static bool ContainsAny(string? value, IEnumerable<string> keywords)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var normalized = value.ToLowerInvariant();
        return keywords.Any(normalized.Contains);
    }

    private static string CalculateStatus(DateTime dueDate, DateTime? completedDate, DateTime today)
    {
        if (completedDate.HasValue && completedDate.Value.Date >= dueDate.Date.AddDays(-45))
            return "Completed";

        if (dueDate.Date < today)
            return "Overdue";

        if (dueDate.Date <= today.AddDays(30))
            return "DueSoon";

        return "Upcoming";
    }
}
