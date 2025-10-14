using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.Services.Interfaces;
using PetCare.Application.DTOs.Pet;
using System.Security.Claims;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PetsController : ControllerBase
{
    private readonly IPetService _petService;

    public PetsController(IPetService petService)
    {
        _petService = petService;
    }

    /// <summary>
    /// Get all pets for the authenticated user
    /// </summary>
    [HttpGet("my-pets")]
    public async Task<IActionResult> GetMyPets()
    {
        var userId = GetUserId();
        var result = await _petService.GetPetsByUserIdAsync(userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get active pets for the authenticated user
    /// </summary>
    [HttpGet("my-pets/active")]
    public async Task<IActionResult> GetMyActivePets()
    {
        var userId = GetUserId();
        var result = await _petService.GetActivePetsAsync(userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get paginated pets for the authenticated user
    /// </summary>
    [HttpGet("my-pets/paged")]
    public async Task<IActionResult> GetMyPetsPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { success = false, message = "Invalid pagination parameters" });
        }

        var userId = GetUserId();
        var result = await _petService.GetPagedPetsAsync(userId, page, pageSize);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get a specific pet by ID (must belong to authenticated user)
    /// </summary>
    [HttpGet("{petId}")]
    public async Task<IActionResult> GetPetById(Guid petId)
    {
        var userId = GetUserId();
        var result = await _petService.GetPetByIdAsync(petId, userId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new pet for the authenticated user
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePet([FromBody] CreatePetDto createPetDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId();
        var result = await _petService.CreatePetAsync(createPetDto, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetPetById), new { petId = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update an existing pet (must belong to authenticated user)
    /// </summary>
    [HttpPut("{petId}")]
    public async Task<IActionResult> UpdatePet(Guid petId, [FromBody] UpdatePetDto updatePetDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId();
        var result = await _petService.UpdatePetAsync(petId, updatePetDto, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete (soft delete) a pet (must belong to authenticated user)
    /// </summary>
    [HttpDelete("{petId}")]
    public async Task<IActionResult> DeletePet(Guid petId)
    {
        var userId = GetUserId();
        var result = await _petService.DeletePetAsync(petId, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        return userId;
    }
}
