using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCare.Application.DTOs.Order;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Data;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CheckoutController : ControllerBase
{
    private readonly PetCareDbContext _context;

    public CheckoutController(PetCareDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var userId = GetUserId();

        var cartItems = await _context.CartItems
            .AsNoTracking()
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        var items = cartItems.Select(c =>
        {
            var unitPrice = c.Product.SalePrice ?? c.Product.Price;
            return new
            {
                c.Id,
                c.ProductId,
                ProductName = c.Product.ProductName,
                c.Quantity,
                UnitPrice = unitPrice,
                TotalPrice = unitPrice * c.Quantity
            };
        }).ToList();

        var totalAmount = items.Sum(i => i.TotalPrice);

        return Ok(new
        {
            success = true,
            message = "Checkout summary retrieved successfully",
            data = new
            {
                items,
                totalAmount,
                shippingFee = 0m,
                finalAmount = totalAmount
            }
        });
    }

    [HttpPost("place-order")]
    public async Task<IActionResult> PlaceOrder([FromBody] CheckoutDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ShippingName)
            || string.IsNullOrWhiteSpace(dto.ShippingPhone)
            || string.IsNullOrWhiteSpace(dto.ShippingAddress))
        {
            return BadRequest(new
            {
                success = false,
                message = "Shipping name, phone, and address are required"
            });
        }

        var paymentMethod = (dto.PaymentMethod ?? "cod").Trim().ToLowerInvariant();
        if (paymentMethod != "cod")
        {
            return BadRequest(new
            {
                success = false,
                message = "Only COD payment is currently supported for product checkout"
            });
        }

        var userId = GetUserId();

        var cartItems = await _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (cartItems.Count == 0)
        {
            return BadRequest(new { success = false, message = "Cart is empty" });
        }

        foreach (var item in cartItems)
        {
            if (!item.Product.IsActive)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Product '{item.Product.ProductName}' is not available"
                });
            }

            if (item.Quantity > item.Product.StockQuantity)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Insufficient stock for '{item.Product.ProductName}'"
                });
            }
        }

        var now = DateTime.UtcNow;
        var orderNumber = $"ORD{now:yyyyMMddHHmmssfff}";

        var totalAmount = cartItems.Sum(i => (i.Product.SalePrice ?? i.Product.Price) * i.Quantity);
        const decimal shippingFee = 0m;
        const decimal discountAmount = 0m;
        var finalAmount = totalAmount + shippingFee - discountAmount;

        var order = new Order
        {
            UserId = userId,
            OrderNumber = orderNumber,
            OrderStatus = "pending",
            TotalAmount = totalAmount,
            ShippingFee = shippingFee,
            DiscountAmount = discountAmount,
            FinalAmount = finalAmount,
            PaymentMethod = paymentMethod,
            PaymentStatus = "unpaid",
            ShippingName = dto.ShippingName.Trim(),
            ShippingPhone = dto.ShippingPhone.Trim(),
            ShippingAddress = dto.ShippingAddress.Trim(),
            Notes = dto.Note?.Trim(),
            OrderedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.Orders.AddAsync(order);

        foreach (var cartItem in cartItems)
        {
            var unitPrice = cartItem.Product.SalePrice ?? cartItem.Product.Price;

            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                ProductName = cartItem.Product.ProductName,
                Quantity = cartItem.Quantity,
                UnitPrice = unitPrice,
                TotalPrice = unitPrice * cartItem.Quantity,
                CreatedAt = now
            };

            await _context.OrderItems.AddAsync(orderItem);

            cartItem.Product.StockQuantity -= cartItem.Quantity;
            cartItem.Product.UpdatedAt = now;
            _context.Products.Update(cartItem.Product);
        }

        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Order placed successfully",
            data = new
            {
                order.Id,
                order.OrderNumber,
                order.FinalAmount,
                order.PaymentMethod,
                order.PaymentStatus,
                order.OrderStatus,
                order.OrderedAt
            }
        });
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }

        return userId;
    }
}
