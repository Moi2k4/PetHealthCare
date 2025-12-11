using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetCare.Application.Common;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Data;

namespace PetCare.Application.Services.Implementations;

public class ServiceManagementService : IServiceManagementService
{
    private readonly PetCareDbContext _context;
    private readonly ILogger<ServiceManagementService> _logger;

    public ServiceManagementService(PetCareDbContext context, ILogger<ServiceManagementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceResult<Service>> CreateServiceAsync(string serviceName, string description, Guid categoryId, int durationMinutes, decimal price, bool isHomeService = false)
    {
        try
        {
            var categoryExists = await _context.ServiceCategories.AnyAsync(c => c.Id == categoryId);
            if (!categoryExists)
                return ServiceResult<Service>.FailureResult("Service category not found");

            var service = new Service
            {
                ServiceName = serviceName,
                Description = description,
                CategoryId = categoryId,
                DurationMinutes = durationMinutes,
                Price = price,
                IsHomeService = isHomeService,
                IsActive = true
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            var result = await _context.Services
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == service.Id);

            return ServiceResult<Service>.SuccessResult(result!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service");
            return ServiceResult<Service>.FailureResult($"Error creating service: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Service>> UpdateServiceAsync(Guid id, string? serviceName = null, string? description = null, decimal? price = null, int? durationMinutes = null)
    {
        try
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
                return ServiceResult<Service>.FailureResult("Service not found");

            if (!string.IsNullOrWhiteSpace(serviceName))
                service.ServiceName = serviceName;

            if (description != null)
                service.Description = description;

            if (price.HasValue)
                service.Price = price.Value;

            if (durationMinutes.HasValue)
                service.DurationMinutes = durationMinutes.Value;

            await _context.SaveChangesAsync();

            var result = await _context.Services
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id);

            return ServiceResult<Service>.SuccessResult(result!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service: {ServiceId}", id);
            return ServiceResult<Service>.FailureResult($"Error updating service: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteServiceAsync(Guid id)
    {
        try
        {
            var service = await _context.Services
                .Include(s => s.Appointments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
                return ServiceResult<bool>.FailureResult("Service not found");

            if (service.Appointments.Any(a => a.AppointmentStatus == "Scheduled" || a.AppointmentStatus == "Confirmed"))
                return ServiceResult<bool>.FailureResult("Cannot delete service with active appointments");

            service.IsActive = false;
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting service: {ServiceId}", id);
            return ServiceResult<bool>.FailureResult($"Error deleting service: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Service>> GetServiceByIdAsync(Guid id)
    {
        try
        {
            var service = await _context.Services
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
                return ServiceResult<Service>.FailureResult("Service not found");

            return ServiceResult<Service>.SuccessResult(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service: {ServiceId}", id);
            return ServiceResult<Service>.FailureResult($"Error retrieving service: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PagedResult<Service>>> GetAllServicesAsync(int page = 1, int pageSize = 10, bool activeOnly = true)
    {
        try
        {
            var query = _context.Services
                .Include(s => s.Category)
                .AsQueryable();

            if (activeOnly)
                query = query.Where(s => s.IsActive);

            var totalCount = await query.CountAsync();
            var services = await query
                .OrderBy(s => s.ServiceName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<Service>
            {
                Items = services,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<Service>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting services");
            return ServiceResult<PagedResult<Service>>.FailureResult($"Error retrieving services: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<Service>>> GetServicesByCategoryAsync(Guid categoryId)
    {
        try
        {
            var services = await _context.Services
                .Include(s => s.Category)
                .Where(s => s.CategoryId == categoryId && s.IsActive)
                .OrderBy(s => s.ServiceName)
                .ToListAsync();

            return ServiceResult<List<Service>>.SuccessResult(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting services by category: {CategoryId}", categoryId);
            return ServiceResult<List<Service>>.FailureResult($"Error retrieving services: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ServiceCategory>> CreateServiceCategoryAsync(string categoryName, string? description = null, string? iconUrl = null)
    {
        try
        {
            var category = new ServiceCategory
            {
                CategoryName = categoryName,
                Description = description,
                IconUrl = iconUrl
            };

            _context.ServiceCategories.Add(category);
            await _context.SaveChangesAsync();

            return ServiceResult<ServiceCategory>.SuccessResult(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service category");
            return ServiceResult<ServiceCategory>.FailureResult($"Error creating service category: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<ServiceCategory>>> GetAllServiceCategoriesAsync()
    {
        try
        {
            var categories = await _context.ServiceCategories
                .Include(c => c.Services.Where(s => s.IsActive))
                .ToListAsync();

            return ServiceResult<List<ServiceCategory>>.SuccessResult(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service categories");
            return ServiceResult<List<ServiceCategory>>.FailureResult($"Error retrieving service categories: {ex.Message}");
        }
    }
}



