using PetCare.Application.Common;
using PetCare.Application.DTOs.Order;

namespace PetCare.Application.Services.Interfaces;

public interface IOrderService
{
    Task<ServiceResult<OrderDto>> GetOrderByIdAsync(Guid orderId);
    Task<ServiceResult<PagedResult<OrderDto>>> GetOrdersAsync(int page, int pageSize);
    Task<ServiceResult<PagedResult<OrderDto>>> GetUserOrdersAsync(Guid userId, int page, int pageSize);
    Task<ServiceResult<OrderDto>> CreateOrderAsync(Guid userId, CreateOrderDto createOrderDto);
    Task<ServiceResult<OrderDto>> CheckoutFromCartAsync(Guid userId, CheckoutDto checkoutDto);
    Task<ServiceResult<OrderDto>> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto updateDto);
    Task<ServiceResult<bool>> CancelOrderAsync(Guid orderId, Guid userId);
}
