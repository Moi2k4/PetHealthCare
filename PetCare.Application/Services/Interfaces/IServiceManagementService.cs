using PetCare.Application.Common;
using PetCare.Domain.Entities;

namespace PetCare.Application.Services.Interfaces;

public interface IServiceManagementService
{
    Task<ServiceResult<Service>> CreateServiceAsync(string serviceName, string description, Guid categoryId, int durationMinutes, decimal price, bool isHomeService = false);
    Task<ServiceResult<Service>> UpdateServiceAsync(Guid id, string? serviceName = null, string? description = null, decimal? price = null, int? durationMinutes = null);
    Task<ServiceResult<bool>> DeleteServiceAsync(Guid id);
    Task<ServiceResult<Service>> GetServiceByIdAsync(Guid id);
    Task<ServiceResult<PagedResult<Service>>> GetAllServicesAsync(int page = 1, int pageSize = 10, bool activeOnly = true);
    Task<ServiceResult<List<Service>>> GetServicesByCategoryAsync(Guid categoryId);
    Task<ServiceResult<ServiceCategory>> CreateServiceCategoryAsync(string categoryName, string? description = null, string? iconUrl = null);
    Task<ServiceResult<List<ServiceCategory>>> GetAllServiceCategoriesAsync();
}
