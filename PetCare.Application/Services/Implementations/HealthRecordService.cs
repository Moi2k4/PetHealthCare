using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using PetCare.Application.Common;
using PetCare.Application.DTOs.Health;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Repositories.Interfaces;

namespace PetCare.Application.Services.Implementations;

public class HealthRecordService : IHealthRecordService
{
    private static readonly string[] DhppKeywords =
    {
        "dhpp", "dhlpp", "distemper", "parvo", "5 in 1", "5in1", "7 in 1", "7in1", "5 benh", "7 benh"
    };

    private static readonly string[] RabiesKeywords =
    {
        "rabies", "dai"
    };

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

    public async Task<ServiceResult<IEnumerable<VaccineCatalogDto>>> GetVaccineCatalogAsync()
    {
        try
        {
            var catalog = await _unitOfWork.Repository<VaccineCatalog>()
                .FindAsync(v => v.IsActive);

            var items = catalog
                .OrderBy(v => v.DisplayName)
                .Select(v => new VaccineCatalogDto
                {
                    Code = v.Code,
                    DisplayName = v.DisplayName,
                    Aliases = SplitAliases(v.Aliases),
                    DefaultIntervalDays = v.DefaultIntervalDays
                })
                .ToList();

            return ServiceResult<IEnumerable<VaccineCatalogDto>>.SuccessResult(items);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<VaccineCatalogDto>>.FailureResult($"Error retrieving vaccine catalog: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<VaccinationDto>>> GetVaccinationsByPetAsync(Guid petId, Guid requestingUserId)
    {
        try
        {
            var pet = await _unitOfWork.Pets.GetByIdAsync(petId);
            if (pet == null)
                return ServiceResult<IEnumerable<VaccinationDto>>.FailureResult("Pet not found");

            if (pet.UserId != requestingUserId)
            {
                var user = await _unitOfWork.Users.GetUserWithRoleAsync(requestingUserId);
                var role = user?.Role?.RoleName?.ToLowerInvariant();
                if (role != "admin" && role != "staff")
                    return ServiceResult<IEnumerable<VaccinationDto>>.FailureResult("You don't have permission to view this pet's vaccination history");
            }

            var vaccinations = await _unitOfWork.Repository<Vaccination>()
                .FindAsync(v => v.PetId == petId);

            var result = vaccinations
                .OrderByDescending(v => v.VaccinationDate)
                .Select(v => new VaccinationDto
                {
                    Id = v.Id,
                    PetId = v.PetId,
                    VaccineCode = v.VaccineCode,
                    VaccineName = v.VaccineName,
                    VaccinationDate = v.VaccinationDate,
                    NextDueDate = v.NextDueDate,
                    BatchNumber = v.BatchNumber,
                    AdministeredBy = v.AdministeredBy,
                    Notes = v.Notes,
                    CreatedAt = v.CreatedAt
                })
                .ToList();

            return ServiceResult<IEnumerable<VaccinationDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<VaccinationDto>>.FailureResult($"Error retrieving vaccination history: {ex.Message}");
        }
    }

    public async Task<ServiceResult<VaccinationDto>> AddVaccinationAsync(Guid petId, CreateVaccinationDto dto, Guid requestingUserId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.VaccineName) && string.IsNullOrWhiteSpace(dto.VaccineCode))
                return ServiceResult<VaccinationDto>.FailureResult("Vaccine code or vaccine name is required");

            if (!string.IsNullOrWhiteSpace(dto.VaccineCode) && dto.VaccineCode.Trim().Length > 50)
                return ServiceResult<VaccinationDto>.FailureResult("Vaccine code must be at most 50 characters");

            if (!string.IsNullOrWhiteSpace(dto.Notes) && dto.Notes.Length > 1000)
                return ServiceResult<VaccinationDto>.FailureResult("Notes must be at most 1000 characters");

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

            var vaccinationDate = EnsureUtc(dto.VaccinationDate ?? DateTime.UtcNow);
            if (vaccinationDate.Date > DateTime.UtcNow.Date)
                return ServiceResult<VaccinationDto>.FailureResult("Vaccination date cannot be in the future");

            if (dto.NextDueDate.HasValue && EnsureUtc(dto.NextDueDate.Value).Date < vaccinationDate.Date)
                return ServiceResult<VaccinationDto>.FailureResult("Next due date cannot be earlier than vaccination date");

            var catalogEntry = await ResolveCatalogEntryAsync(dto.VaccineCode, dto.VaccineName);
            var resolvedCode = catalogEntry?.Code;
            var resolvedName = catalogEntry?.DisplayName ?? dto.VaccineName.Trim();

            if (resolvedName.Length > 255)
                return ServiceResult<VaccinationDto>.FailureResult("Vaccine name must be at most 255 characters");

            if (string.IsNullOrWhiteSpace(resolvedName))
                return ServiceResult<VaccinationDto>.FailureResult("Vaccine name is required");

            DateTime? nextDueDate = dto.NextDueDate.HasValue
                ? EnsureUtc(dto.NextDueDate.Value)
                : await EstimateNextDueDateAsync(petId, resolvedCode, resolvedName, vaccinationDate, catalogEntry?.DefaultIntervalDays);

            var entity = new Vaccination
            {
                PetId = petId,
                VaccineCode = resolvedCode,
                VaccineName = resolvedName,
                VaccinationDate = vaccinationDate,
                NextDueDate = nextDueDate,
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
                VaccineCode = entity.VaccineCode,
                VaccineName = entity.VaccineName,
                VaccinationDate = entity.VaccinationDate,
                NextDueDate = entity.NextDueDate,
                BatchNumber = entity.BatchNumber,
                AdministeredBy = entity.AdministeredBy,
                Notes = entity.Notes,
                CreatedAt = entity.CreatedAt
            };

            var message = nextDueDate.HasValue
                ? $"Vaccination recorded successfully. Estimated next dose: {nextDueDate.Value:yyyy-MM-dd}"
                : "Vaccination recorded successfully";

            return ServiceResult<VaccinationDto>.SuccessResult(result, message);
        }
        catch (Exception ex)
        {
            var detail = ex.InnerException?.Message ?? ex.Message;
            return ServiceResult<VaccinationDto>.FailureResult($"Error recording vaccination: {detail}");
        }
    }

    private static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    private async Task<VaccineCatalog?> ResolveCatalogEntryAsync(string? vaccineCode, string vaccineName)
    {
        var catalog = await _unitOfWork.Repository<VaccineCatalog>()
            .FindAsync(v => v.IsActive);

        if (!string.IsNullOrWhiteSpace(vaccineCode))
        {
            var byCode = catalog.FirstOrDefault(v =>
                string.Equals(v.Code, vaccineCode.Trim(), StringComparison.OrdinalIgnoreCase));
            if (byCode != null) return byCode;
        }

        if (string.IsNullOrWhiteSpace(vaccineName)) return null;

        var normalizedName = NormalizeText(vaccineName);
        return catalog.FirstOrDefault(v =>
            NormalizeText(v.DisplayName) == normalizedName ||
            SplitAliases(v.Aliases).Any(alias => NormalizeText(alias) == normalizedName));
    }

    private static string[] SplitAliases(string? aliases)
    {
        if (string.IsNullOrWhiteSpace(aliases)) return [];
        return aliases
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
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
        var normalizedValue = NormalizeText(value);
        return keywords.Any(keyword => normalizedValue.Contains(NormalizeText(keyword)));
    }

    private async Task<DateTime?> EstimateNextDueDateAsync(
        Guid petId,
        string? vaccineCode,
        string vaccineName,
        DateTime vaccinationDate,
        int? defaultIntervalDays)
    {
        // Keep explicit user input as source of truth; estimate only when next due is omitted.
        if (string.Equals(vaccineCode, "RABIES", StringComparison.OrdinalIgnoreCase)
            || ContainsAny(vaccineName, RabiesKeywords))
        {
            return vaccinationDate.Date.AddYears(1);
        }

        if (string.Equals(vaccineCode, "DHPP", StringComparison.OrdinalIgnoreCase)
            || ContainsAny(vaccineName, DhppKeywords))
        {
            var existingVaccinations = await _unitOfWork.Repository<Vaccination>()
                .FindAsync(v => v.PetId == petId);

            var previousDoseCount = existingVaccinations.Count(v => ContainsAny(v.VaccineName, DhppKeywords));

            // Practical schedule:
            // dose 1 -> +14 days, dose 2 -> +28 days, then annual boosters.
            if (previousDoseCount <= 0) return vaccinationDate.Date.AddDays(14);
            if (previousDoseCount == 1) return vaccinationDate.Date.AddDays(28);
            return vaccinationDate.Date.AddYears(1);
        }

        if (defaultIntervalDays.HasValue && defaultIntervalDays.Value > 0)
        {
            return vaccinationDate.Date.AddDays(defaultIntervalDays.Value);
        }

        // Unknown vaccine naming: no hard estimate to avoid unsafe reminders.
        return null;
    }

    private static string NormalizeText(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToLowerInvariant(c));
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
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
