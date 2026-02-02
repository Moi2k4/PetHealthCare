namespace PetCare.Application.DTOs.Dashboard;

public class DashboardStatisticsDto
{
    public int TotalUsers { get; set; }
    public int TotalPets { get; set; }
    public int TotalOrders { get; set; }
    public int TotalAppointments { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int PendingAppointments { get; set; }
    public int ActiveSubscriptions { get; set; }
    public List<RevenueByMonthDto> RevenueByMonth { get; set; } = new();
    public List<TopProductDto> TopSellingProducts { get; set; } = new();
    public List<TopServiceDto> PopularServices { get; set; } = new();
    public List<OrderStatusCountDto> OrderStatusCounts { get; set; } = new();
}

public class RevenueByMonthDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class TopProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int SoldQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class TopServiceDto
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class OrderStatusCountDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class UserStatisticsDto
{
    public int TotalMyPets { get; set; }
    public int TotalMyOrders { get; set; }
    public int TotalMyAppointments { get; set; }
    public decimal TotalSpent { get; set; }
    public bool HasActiveSubscription { get; set; }
    public List<RecentOrderDto> RecentOrders { get; set; } = new();
    public List<UpcomingAppointmentDto> UpcomingAppointments { get; set; } = new();
}

public class RecentOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public decimal FinalAmount { get; set; }
    public DateTime OrderedAt { get; set; }
}

public class UpcomingAppointmentDto
{
    public Guid Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string PetName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
