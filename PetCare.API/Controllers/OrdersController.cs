using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.DTOs.Order;
using PetCare.Application.Services.Interfaces;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private bool IsAdmin()
    {
        return User.IsInRole("admin");
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _orderService.GetOrderByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get all orders (admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _orderService.GetOrdersAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get current user's orders
    /// </summary>
    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await _orderService.GetUserOrdersAsync(userId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Create new order
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto createOrderDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await _orderService.CreateOrderAsync(userId, createOrderDto);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Create order from cart
    /// </summary>
    [HttpPost("checkout")]
    public async Task<IActionResult> CheckoutFromCart([FromBody] CheckoutDto checkoutDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await _orderService.CheckoutFromCartAsync(userId, checkoutDto);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update order status (admin only)
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "admin,staff")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _orderService.UpdateOrderStatusAsync(id, updateDto);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Cancel order
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await _orderService.CancelOrderAsync(id, userId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
