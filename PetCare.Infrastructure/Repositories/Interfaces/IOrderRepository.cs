using PetCare.Domain.Entities;

namespace PetCare.Infrastructure.Repositories.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId);
    Task<Order?> GetOrderWithItemsAsync(Guid orderId);
    Task<Order?> GetOrderByNumberAsync(string orderNumber);
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status);
}
