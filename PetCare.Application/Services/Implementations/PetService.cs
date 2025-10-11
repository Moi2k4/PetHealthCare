using AutoMapper;
using PetCare.Application.DTOs.Pet;
using PetCare.Application.Common;
using PetCare.Application.Services.Interfaces;
using PetCare.Infrastructure.Repositories.Interfaces;
using PetCare.Domain.Entities;

namespace PetCare.Application.Services.Implementations;

public class PetService : IPetService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PetService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<PetDto>> GetPetByIdAsync(Guid petId)
    {
        try
        {
            var pet = await _unitOfWork.Pets.GetPetWithDetailsAsync(petId);
            
            if (pet == null)
            {
                return ServiceResult<PetDto>.FailureResult("Pet not found");
            }

            var petDto = _mapper.Map<PetDto>(pet);
            return ServiceResult<PetDto>.SuccessResult(petDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<PetDto>.FailureResult($"Error retrieving pet: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<PetDto>>> GetPetsByUserIdAsync(Guid userId)
    {
        try
        {
            var pets = await _unitOfWork.Pets.GetPetsByUserIdAsync(userId);
            var petDtos = _mapper.Map<IEnumerable<PetDto>>(pets);
            
            return ServiceResult<IEnumerable<PetDto>>.SuccessResult(petDtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<PetDto>>.FailureResult($"Error retrieving pets: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PetDto>> CreatePetAsync(CreatePetDto createPetDto)
    {
        try
        {
            var pet = _mapper.Map<Pet>(createPetDto);
            await _unitOfWork.Pets.AddAsync(pet);
            await _unitOfWork.SaveChangesAsync();

            var petDto = _mapper.Map<PetDto>(pet);
            return ServiceResult<PetDto>.SuccessResult(petDto, "Pet created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<PetDto>.FailureResult($"Error creating pet: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PetDto>> UpdatePetAsync(Guid petId, CreatePetDto updatePetDto)
    {
        try
        {
            var pet = await _unitOfWork.Pets.GetByIdAsync(petId);
            
            if (pet == null)
            {
                return ServiceResult<PetDto>.FailureResult("Pet not found");
            }

            _mapper.Map(updatePetDto, pet);
            await _unitOfWork.Pets.UpdateAsync(pet);
            await _unitOfWork.SaveChangesAsync();

            var petDto = _mapper.Map<PetDto>(pet);
            return ServiceResult<PetDto>.SuccessResult(petDto, "Pet updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<PetDto>.FailureResult($"Error updating pet: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeletePetAsync(Guid petId)
    {
        try
        {
            var pet = await _unitOfWork.Pets.GetByIdAsync(petId);
            
            if (pet == null)
            {
                return ServiceResult<bool>.FailureResult("Pet not found");
            }

            await _unitOfWork.Pets.DeleteAsync(pet);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true, "Pet deleted successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error deleting pet: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<PetDto>>> GetActivePetsAsync(Guid userId)
    {
        try
        {
            var pets = await _unitOfWork.Pets.GetActivePetsAsync(userId);
            var petDtos = _mapper.Map<IEnumerable<PetDto>>(pets);
            
            return ServiceResult<IEnumerable<PetDto>>.SuccessResult(petDtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<PetDto>>.FailureResult($"Error retrieving active pets: {ex.Message}");
        }
    }
}
