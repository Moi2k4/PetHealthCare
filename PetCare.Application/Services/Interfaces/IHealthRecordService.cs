using PetCare.Application.Common;
using PetCare.Application.DTOs.Health;

namespace PetCare.Application.Services.Interfaces;

public interface IHealthRecordService
{
    Task<ServiceResult<IEnumerable<HealthRecordDto>>> GetByPetAsync(Guid petId, Guid requestingUserId);
    Task<ServiceResult<HealthRecordDto>> GetByIdAsync(Guid recordId, Guid requestingUserId);
    Task<ServiceResult<HealthRecordDto>> CreateAsync(CreateHealthRecordDto dto, Guid recordedByUserId);
    Task<ServiceResult<HealthRecordDto>> UpdateAsync(Guid recordId, UpdateHealthRecordDto dto, Guid requestingUserId);
    Task<ServiceResult<bool>> DeleteAsync(Guid recordId, Guid requestingUserId);
    Task<ServiceResult<IEnumerable<VaccineCatalogDto>>> GetVaccineCatalogAsync();
    Task<ServiceResult<IEnumerable<VaccinationDto>>> GetVaccinationsByPetAsync(Guid petId, Guid requestingUserId);
    Task<ServiceResult<VaccinationDto>> AddVaccinationAsync(Guid petId, CreateVaccinationDto dto, Guid requestingUserId);
    Task<ServiceResult<DogRoutineScheduleDto>> GetDogRoutineScheduleAsync(Guid petId, Guid requestingUserId);
}
