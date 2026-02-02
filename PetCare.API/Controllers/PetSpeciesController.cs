using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCare.Application.DTOs.Pet;
using PetCare.Infrastructure.Data;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class PetSpeciesController : ControllerBase
{
    private readonly PetCareDbContext _context;
    private readonly IMapper _mapper;

    public PetSpeciesController(PetCareDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all pet species
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PetSpeciesDto>>> GetAllSpecies()
    {
        var species = await _context.PetSpecies
            .OrderBy(s => s.SpeciesName)
            .ToListAsync();

        var speciesDtos = _mapper.Map<List<PetSpeciesDto>>(species);
        return Ok(speciesDtos);
    }

    /// <summary>
    /// Get all species with their breeds
    /// </summary>
    [HttpGet("with-breeds")]
    public async Task<ActionResult<List<SpeciesWithBreedsDto>>> GetSpeciesWithBreeds()
    {
        var species = await _context.PetSpecies
            .Include(s => s.Breeds)
            .OrderBy(s => s.SpeciesName)
            .ToListAsync();

        var speciesDtos = _mapper.Map<List<SpeciesWithBreedsDto>>(species);
        return Ok(speciesDtos);
    }

    /// <summary>
    /// Get a specific species by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PetSpeciesDto>> GetSpeciesById(Guid id)
    {
        var species = await _context.PetSpecies.FindAsync(id);
        
        if (species == null)
        {
            return NotFound(new { message = "Species not found" });
        }

        var speciesDto = _mapper.Map<PetSpeciesDto>(species);
        return Ok(speciesDto);
    }

    /// <summary>
    /// Get all breeds with optional pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 50, max: 200)</param>
    [HttpGet("breeds")]
    public async Task<ActionResult<object>> GetAllBreeds(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 50)
    {
        pageSize = Math.Min(pageSize, 200); // Max 200 items per page
        pageNumber = Math.Max(pageNumber, 1); // Min page 1

        var totalCount = await _context.PetBreeds.CountAsync();
        
        var breeds = await _context.PetBreeds
            .Include(b => b.Species)
            .OrderBy(b => b.Species.SpeciesName)
            .ThenBy(b => b.BreedName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var breedDtos = _mapper.Map<List<PetBreedDto>>(breeds);
        
        return Ok(new 
        { 
            data = breedDtos,
            pagination = new
            {
                totalCount,
                pageNumber,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            }
        });
    }

    /// <summary>
    /// Get breeds by species ID
    /// </summary>
    [HttpGet("{speciesId}/breeds")]
    public async Task<ActionResult<List<PetBreedDto>>> GetBreedsBySpecies(Guid speciesId)
    {
        var species = await _context.PetSpecies.FindAsync(speciesId);
        if (species == null)
        {
            return NotFound(new { message = "Species not found" });
        }

        var breeds = await _context.PetBreeds
            .Where(b => b.SpeciesId == speciesId)
            .OrderBy(b => b.BreedName)
            .ToListAsync();

        var breedDtos = _mapper.Map<List<PetBreedDto>>(breeds);
        return Ok(breedDtos);
    }

    /// <summary>
    /// Get a specific breed by ID
    /// </summary>
    [HttpGet("breeds/{id}")]
    public async Task<ActionResult<PetBreedDto>> GetBreedById(Guid id)
    {
        var breed = await _context.PetBreeds
            .Include(b => b.Species)
            .FirstOrDefaultAsync(b => b.Id == id);
        
        if (breed == null)
        {
            return NotFound(new { message = "Breed not found" });
        }

        var breedDto = _mapper.Map<PetBreedDto>(breed);
        return Ok(breedDto);
    }
}
