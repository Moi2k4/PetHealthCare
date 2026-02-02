namespace PetCare.Application.Services.Interfaces;

using PetCare.Application.Common;
using PetCare.Application.DTOs.Dashboard;

public interface IDashboardService
{
    Task<ServiceResult<DashboardStatisticsDto>> GetAdminDashboardAsync();
    Task<ServiceResult<UserStatisticsDto>> GetUserDashboardAsync(Guid userId);
    Task<ServiceResult<List<RevenueByMonthDto>>> GetRevenueByMonthAsync(int year);
}
