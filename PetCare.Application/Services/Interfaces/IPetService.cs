using PetCare.Application.DTOs.Pet;
using PetCare.Application.Common;

namespace PetCare.Application.Services.Interfaces;

public interface IPetService
{
    Task<ServiceResult<PetDto>> GetPetByIdAsync(Guid petId, Guid userId);
    Task<ServiceResult<IEnumerable<PetDto>>> GetPetsByUserIdAsync(Guid userId);
    Task<ServiceResult<PetDto>> CreatePetAsync(CreatePetDto createPetDto, Guid userId);
    Task<ServiceResult<PetDto>> UpdatePetAsync(Guid petId, UpdatePetDto updatePetDto, Guid userId);
    Task<ServiceResult<bool>> DeletePetAsync(Guid petId, Guid userId);
    Task<ServiceResult<IEnumerable<PetDto>>> GetActivePetsAsync(Guid userId);
    Task<ServiceResult<PagedResult<PetDto>>> GetPagedPetsAsync(Guid userId, int page, int pageSize);
}
