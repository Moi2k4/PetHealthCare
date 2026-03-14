using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCare.Infrastructure.Data;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly PetCareDbContext _context;

    public AdminDashboardController(PetCareDbContext context)
    {
        _context = context;
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueOverview()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthStart = monthStart.AddMonths(1);

        var paidOrdersQuery = _context.Orders
            .AsNoTracking()
            .Where(order =>
                (order.PaymentStatus != null &&
                 (order.PaymentStatus.ToLower() == "paid" || order.PaymentStatus.ToLower() == "completed")) ||
                (order.PaymentMethod != null && order.PaymentMethod.ToLower() == "cod" &&
                 order.OrderStatus != null &&
                 (order.OrderStatus.ToLower() == "completed" || order.OrderStatus.ToLower() == "delivered")));

        var totalRevenue = await paidOrdersQuery.SumAsync(order => (decimal?)order.FinalAmount) ?? 0m;
        var paidOrders = await paidOrdersQuery.CountAsync();
        var paidRevenueThisMonth = await paidOrdersQuery
            .Where(order => order.OrderedAt >= monthStart && order.OrderedAt < nextMonthStart)
            .SumAsync(order => (decimal?)order.FinalAmount) ?? 0m;

        var totalOrders = await _context.Orders.AsNoTracking().CountAsync();

        return Ok(new
        {
            success = true,
            message = "Revenue overview retrieved successfully",
            data = new
            {
                totalRevenue,
                paidRevenueThisMonth,
                totalOrders,
                paidOrders,
                generatedAt = now
            }
        });
    }
}