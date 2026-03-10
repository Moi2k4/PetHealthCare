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
}
