using Microsoft.AspNetCore.Mvc;
using PetCare.Application.Services.Interfaces;
using PetCare.Application.DTOs.Pet;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PetsController : ControllerBase
{
    private readonly IPetService _petService;

    public PetsController(IPetService petService)
    {
        _petService = petService;
    }

    /// <summary>
    /// Get pet by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _petService.GetPetByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Get pets by user ID
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var result = await _petService.GetPetsByUserIdAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Get active pets by user ID
    /// </summary>
    [HttpGet("user/{userId}/active")]
    public async Task<IActionResult> GetActivePets(Guid userId)
    {
        var result = await _petService.GetActivePetsAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Create new pet
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePetDto createPetDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _petService.CreatePetAsync(createPetDto);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update pet
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreatePetDto updatePetDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _petService.UpdatePetAsync(id, updatePetDto);
        
        if (!result.Success)
        {
            return NotFound(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Delete pet
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _petService.DeletePetAsync(id);
        
        if (!result.Success)
        {
            return NotFound(result);
        }
        
        return Ok(result);
    }
}
