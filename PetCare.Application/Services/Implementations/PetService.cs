using AutoMapper;
using PetCare.Application.DTOs.Pet;
using PetCare.Application.Common;
using PetCare.Application.Services.Interfaces;
using PetCare.Infrastructure.Repositories.Interfaces;
using PetCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

    public async Task<ServiceResult<PetDto>> GetPetByIdAsync(Guid petId, Guid userId)
    {
        try
        {
            var pet = await _unitOfWork.Pets.GetPetWithDetailsAsync(petId);
            
            if (pet == null)
            {
                return ServiceResult<PetDto>.FailureResult("Pet not found");
            }

            // Verify pet belongs to the user
            if (pet.UserId != userId)
            {
                return ServiceResult<PetDto>.FailureResult("You don't have permission to access this pet");
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

    public async Task<ServiceResult<PetDto>> CreatePetAsync(CreatePetDto createPetDto, Guid userId)
    {
        try
        {
            // Validate species exists if provided
            if (createPetDto.SpeciesId.HasValue)
            {
                var speciesExists = await _unitOfWork.Repository<PetSpecies>()
                    .AnyAsync(s => s.Id == createPetDto.SpeciesId.Value);
                if (!speciesExists)
                {
                    return ServiceResult<PetDto>.FailureResult("Invalid species ID");
                }
            }

            // Validate breed exists if provided
            if (createPetDto.BreedId.HasValue)
            {
                var breedExists = await _unitOfWork.Repository<PetBreed>()
                    .AnyAsync(b => b.Id == createPetDto.BreedId.Value);
                if (!breedExists)
                {
                    return ServiceResult<PetDto>.FailureResult("Invalid breed ID");
                }
            }

            var pet = _mapper.Map<Pet>(createPetDto);
            pet.UserId = userId; // Set the authenticated user as owner
            pet.IsActive = true;
            
            await _unitOfWork.Pets.AddAsync(pet);
            await _unitOfWork.SaveChangesAsync();

            // Reload with details
            var createdPet = await _unitOfWork.Pets.GetPetWithDetailsAsync(pet.Id);
            var petDto = _mapper.Map<PetDto>(createdPet);
            
            return ServiceResult<PetDto>.SuccessResult(petDto, "Pet created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<PetDto>.FailureResult($"Error creating pet: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PetDto>> UpdatePetAsync(Guid petId, UpdatePetDto updatePetDto, Guid userId)
    {
        try
        {
            var pet = await _unitOfWork.Pets.GetByIdAsync(petId);
            
            if (pet == null)
            {
                return ServiceResult<PetDto>.FailureResult("Pet not found");
            }

            // Verify pet belongs to the user
            if (pet.UserId != userId)
            {
                return ServiceResult<PetDto>.FailureResult("You don't have permission to update this pet");
            }

            // Validate species exists if provided
            if (updatePetDto.SpeciesId.HasValue)
            {
                var speciesExists = await _unitOfWork.Repository<PetSpecies>()
                    .AnyAsync(s => s.Id == updatePetDto.SpeciesId.Value);
                if (!speciesExists)
                {
                    return ServiceResult<PetDto>.FailureResult("Invalid species ID");
                }
            }

            // Validate breed exists if provided
            if (updatePetDto.BreedId.HasValue)
            {
                var breedExists = await _unitOfWork.Repository<PetBreed>()
                    .AnyAsync(b => b.Id == updatePetDto.BreedId.Value);
                if (!breedExists)
                {
                    return ServiceResult<PetDto>.FailureResult("Invalid breed ID");
                }
            }

            _mapper.Map(updatePetDto, pet);
            await _unitOfWork.Pets.UpdateAsync(pet);
            await _unitOfWork.SaveChangesAsync();

            // Reload with details
            var updatedPet = await _unitOfWork.Pets.GetPetWithDetailsAsync(petId);
            var petDto = _mapper.Map<PetDto>(updatedPet);
            
            return ServiceResult<PetDto>.SuccessResult(petDto, "Pet updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<PetDto>.FailureResult($"Error updating pet: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeletePetAsync(Guid petId, Guid userId)
    {
        try
        {
            var pet = await _unitOfWork.Pets.GetByIdAsync(petId);
            
            if (pet == null)
            {
                return ServiceResult<bool>.FailureResult("Pet not found");
            }

            // Verify pet belongs to the user
            if (pet.UserId != userId)
            {
                return ServiceResult<bool>.FailureResult("You don't have permission to delete this pet");
            }

            // Soft delete by setting IsActive to false
            pet.IsActive = false;
            await _unitOfWork.Pets.UpdateAsync(pet);
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

    public async Task<ServiceResult<PagedResult<PetDto>>> GetPagedPetsAsync(Guid userId, int page, int pageSize)
    {
        try
        {
            var query = await _unitOfWork.Pets.GetPetsByUserIdAsync(userId);
            
            var totalItems = query.Count();
            var pets = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var petDtos = _mapper.Map<IEnumerable<PetDto>>(pets);
            
            var pagedResult = new PagedResult<PetDto>
            {
                Items = petDtos,
                TotalCount = totalItems,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<PetDto>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            return ServiceResult<PagedResult<PetDto>>.FailureResult($"Error retrieving pets: {ex.Message}");
        }
    }
}

