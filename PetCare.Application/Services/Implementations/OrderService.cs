using AutoMapper;
using PetCare.Application.Common;
using PetCare.Application.DTOs.Order;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Repositories.Interfaces;

namespace PetCare.Application.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<OrderDto>> GetOrderByIdAsync(Guid orderId)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId);
            if (order == null)
            {
                return ServiceResult<OrderDto>.FailureResult("Order not found");
            }

            var orderDto = _mapper.Map<OrderDto>(order);
            return ServiceResult<OrderDto>.SuccessResult(orderDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<OrderDto>.FailureResult($"Error retrieving order: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PagedResult<OrderDto>>> GetOrdersAsync(int page, int pageSize)
    {
        try
        {
            var (orders, totalCount) = await _unitOfWork.Orders.GetPagedAsync(
                page,
                pageSize,
                orderBy: q => q.OrderByDescending(o => o.CreatedAt),
                includes: o => o.OrderItems);

            var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);

            var pagedResult = new PagedResult<OrderDto>
            {
                Items = orderDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<OrderDto>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            return ServiceResult<PagedResult<OrderDto>>.FailureResult($"Error retrieving orders: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PagedResult<OrderDto>>> GetUserOrdersAsync(Guid userId, int page, int pageSize)
    {
        try
        {
            var (orders, totalCount) = await _unitOfWork.Orders.GetPagedAsync(
                page,
                pageSize,
                filter: o => o.UserId == userId,
                orderBy: q => q.OrderByDescending(o => o.CreatedAt),
                includes: o => o.OrderItems);

            var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);

            var pagedResult = new PagedResult<OrderDto>
            {
                Items = orderDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<OrderDto>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            return ServiceResult<PagedResult<OrderDto>>.FailureResult($"Error retrieving user orders: {ex.Message}");
        }
    }

    public async Task<ServiceResult<OrderDto>> CreateOrderAsync(Guid userId, CreateOrderDto createOrderDto)
    {
        try
        {
            if (createOrderDto.Items == null || !createOrderDto.Items.Any())
            {
                return ServiceResult<OrderDto>.FailureResult("Order must contain at least one item");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    UserId = userId,
                    OrderNumber = GenerateOrderNumber(),
                    OrderStatus = "Pending",
                    PaymentStatus = "Pending",
                    PaymentMethod = createOrderDto.PaymentMethod,
                    ShippingName = createOrderDto.ShippingName,
                    ShippingPhone = createOrderDto.ShippingPhone,
                    ShippingAddress = createOrderDto.ShippingAddress,
                    Notes = createOrderDto.Note,
                    OrderedAt = DateTime.UtcNow
                };

                decimal totalAmount = 0;

                foreach (var itemDto in createOrderDto.Items)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
                    if (product == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return ServiceResult<OrderDto>.FailureResult($"Product {itemDto.ProductId} not found");
                    }

                    if (!product.IsActive)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return ServiceResult<OrderDto>.FailureResult($"Product {product.ProductName} is not available");
                    }

                    if (product.StockQuantity < itemDto.Quantity)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return ServiceResult<OrderDto>.FailureResult($"Insufficient stock for {product.ProductName}");
                    }

                    var unitPrice = product.SalePrice ?? product.Price;
                    var orderItem = new OrderItem
                    {
                        ProductId = product.Id,
                        ProductName = product.ProductName,
                        Quantity = itemDto.Quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = unitPrice * itemDto.Quantity
                    };

                    order.OrderItems.Add(orderItem);
                    totalAmount += orderItem.TotalPrice;

                    // Update stock
                    product.StockQuantity -= itemDto.Quantity;
                    await _unitOfWork.Products.UpdateAsync(product);
                }

                order.TotalAmount = totalAmount;
                order.ShippingFee = CalculateShippingFee(totalAmount);
                order.DiscountAmount = 0;
                order.FinalAmount = order.TotalAmount + order.ShippingFee - order.DiscountAmount;

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                // Clear cart after successful order
                var cartItemRepo = _unitOfWork.Repository<CartItem>();
                var cartItems = await cartItemRepo.FindAsync(c => c.UserId == userId);
                await cartItemRepo.DeleteRangeAsync(cartItems);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                var orderDto = _mapper.Map<OrderDto>(order);
                return ServiceResult<OrderDto>.SuccessResult(orderDto, "Order created successfully");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            return ServiceResult<OrderDto>.FailureResult($"Error creating order: {ex.Message}");
        }
    }

    public async Task<ServiceResult<OrderDto>> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto updateDto)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
            {
                return ServiceResult<OrderDto>.FailureResult("Order not found");
            }

            order.OrderStatus = updateDto.OrderStatus;
            
            if (updateDto.OrderStatus == "Cancelled")
            {
                // Restore stock
                foreach (var item in order.OrderItems)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        await _unitOfWork.Products.UpdateAsync(product);
                    }
                }
            }

            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            var orderDto = _mapper.Map<OrderDto>(order);
            return ServiceResult<OrderDto>.SuccessResult(orderDto, "Order status updated");
        }
        catch (Exception ex)
        {
            return ServiceResult<OrderDto>.FailureResult($"Error updating order status: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> CancelOrderAsync(Guid orderId, Guid userId)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
            {
                return ServiceResult<bool>.FailureResult("Order not found");
            }

            if (order.UserId != userId)
            {
                return ServiceResult<bool>.FailureResult("Unauthorized to cancel this order");
            }

            if (order.OrderStatus != "Pending" && order.OrderStatus != "Confirmed")
            {
                return ServiceResult<bool>.FailureResult("Cannot cancel order in current status");
            }

            var updateResult = await UpdateOrderStatusAsync(orderId, new UpdateOrderStatusDto
            {
                OrderStatus = "Cancelled"
            });

            return updateResult.Success
                ? ServiceResult<bool>.SuccessResult(true, "Order cancelled successfully")
                : ServiceResult<bool>.FailureResult(updateResult.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error cancelling order: {ex.Message}");
        }
    }

    private string GenerateOrderNumber()
    {
        return $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
    }

    private decimal CalculateShippingFee(decimal totalAmount)
    {
        // Simple shipping fee calculation
        if (totalAmount >= 500000) return 0; // Free shipping for orders over 500k
        if (totalAmount >= 300000) return 20000;
        return 30000;
    }
}
