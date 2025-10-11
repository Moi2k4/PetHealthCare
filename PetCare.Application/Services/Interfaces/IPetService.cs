using PetCare.Application.DTOs.Pet;
using PetCare.Application.Common;

namespace PetCare.Application.Services.Interfaces;

public interface IPetService
{
    Task<ServiceResult<PetDto>> GetPetByIdAsync(Guid petId);
    Task<ServiceResult<IEnumerable<PetDto>>> GetPetsByUserIdAsync(Guid userId);
    Task<ServiceResult<PetDto>> CreatePetAsync(CreatePetDto createPetDto);
    Task<ServiceResult<PetDto>> UpdatePetAsync(Guid petId, CreatePetDto updatePetDto);
    Task<ServiceResult<bool>> DeletePetAsync(Guid petId);
    Task<ServiceResult<IEnumerable<PetDto>>> GetActivePetsAsync(Guid userId);
}
