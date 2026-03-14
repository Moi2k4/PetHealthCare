using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCare.Application.DTOs.Product;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Data;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly PetCareDbContext _context;

    public CartController(PetCareDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyCart()
    {
        var userId = GetUserId();

        var items = await _context.CartItems
            .AsNoTracking()
            .Include(c => c.Product)
            .ThenInclude(p => p.Images)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CartItemDto
            {
                Id = c.Id,
                ProductId = c.ProductId,
                ProductName = c.Product.ProductName,
                Price = c.Product.Price,
                SalePrice = c.Product.SalePrice,
                Quantity = c.Quantity,
                StockQuantity = c.Product.StockQuantity,
                ImageUrl = c.Product.Images
                    .OrderByDescending(i => i.IsPrimary)
                    .ThenBy(i => i.DisplayOrder)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            message = "Cart retrieved successfully",
            data = items,
            errors = Array.Empty<string>()
        });
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
    {
        if (dto.ProductId == Guid.Empty)
        {
            return BadRequest(new { success = false, message = "ProductId is required" });
        }

        if (dto.Quantity <= 0)
        {
            return BadRequest(new { success = false, message = "Quantity must be greater than 0" });
        }

        var userId = GetUserId();

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.IsActive);

        if (product == null)
        {
            return NotFound(new { success = false, message = "Product not found" });
        }

        var existing = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == dto.ProductId);

        if (existing == null)
        {
            if (dto.Quantity > product.StockQuantity)
            {
                return BadRequest(new { success = false, message = "Quantity exceeds stock" });
            }

            existing = new CartItem
            {
                UserId = userId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.CartItems.AddAsync(existing);
        }
        else
        {
            var nextQty = existing.Quantity + dto.Quantity;
            if (nextQty > product.StockQuantity)
            {
                return BadRequest(new { success = false, message = "Quantity exceeds stock" });
            }

            existing.Quantity = nextQty;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.CartItems.Update(existing);
        }

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Added to cart", data = existing.Id });
    }

    [HttpPut("{cartItemId:guid}")]
    public async Task<IActionResult> UpdateCartItem(Guid cartItemId, [FromBody] UpdateCartItemDto dto)
    {
        if (dto.Quantity <= 0)
        {
            return BadRequest(new { success = false, message = "Quantity must be greater than 0" });
        }

        var userId = GetUserId();

        var cartItem = await _context.CartItems
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

        if (cartItem == null)
        {
            return NotFound(new { success = false, message = "Cart item not found" });
        }

        if (dto.Quantity > cartItem.Product.StockQuantity)
        {
            return BadRequest(new { success = false, message = "Quantity exceeds stock" });
        }

        cartItem.Quantity = dto.Quantity;
        cartItem.UpdatedAt = DateTime.UtcNow;

        _context.CartItems.Update(cartItem);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Cart item updated" });
    }

    [HttpDelete("{cartItemId:guid}")]
    public async Task<IActionResult> RemoveCartItem(Guid cartItemId, [FromQuery] int quantity = 1)
    {
        var userId = GetUserId();

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

        if (cartItem == null)
        {
            return NotFound(new { success = false, message = "Cart item not found" });
        }

        if (quantity <= 0 || quantity >= cartItem.Quantity)
        {
            _context.CartItems.Remove(cartItem);
        }
        else
        {
            cartItem.Quantity -= quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
            _context.CartItems.Update(cartItem);
        }

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Cart item removed" });
    }

    [HttpDelete]
    public async Task<IActionResult> ClearMyCart()
    {
        var userId = GetUserId();

        var items = await _context.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (items.Count == 0)
        {
            return Ok(new { success = true, message = "Cart already empty" });
        }

        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Cart cleared" });
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
