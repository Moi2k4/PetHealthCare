namespace PetCare.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.DTOs.Voucher;
using PetCare.Application.Services.Interfaces;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class VouchersController : ControllerBase
{
    private readonly IVoucherService _voucherService;

    public VouchersController(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    [HttpPost]
    [Authorize(Roles = "admin,service_provider")]
    public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherDto dto)
    {
        var result = await _voucherService.CreateVoucherAsync(dto);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllVouchers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] bool? isActive = null)
    {
        var result = await _voucherService.GetAllVouchersAsync(pageNumber, pageSize, isActive);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVoucherById(Guid id)
    {
        var result = await _voucherService.GetVoucherByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetVoucherByCode(string code)
    {
        var result = await _voucherService.GetVoucherByCodeAsync(code);
        if (!result.Success)
        {
            return NotFound(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpGet("available")]
    [Authorize]
    public async Task<IActionResult> GetAvailableVouchers()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user token" });
        }

        var result = await _voucherService.GetAvailableVouchersForUserAsync(userId);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpPost("validate")]
    [Authorize]
    public async Task<IActionResult> ValidateVoucher([FromBody] ValidateVoucherDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user token" });
        }

        var result = await _voucherService.ValidateVoucherAsync(dto, userId);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpPost("apply")]
    [Authorize]
    public async Task<IActionResult> ApplyVoucherToOrder([FromBody] ApplyVoucherDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user token" });
        }

        var result = await _voucherService.ApplyVoucherToOrderAsync(dto.OrderId, dto.VoucherCode, userId);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(new { message = "Áp dụng voucher thành công", success = true });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,service_provider")]
    public async Task<IActionResult> UpdateVoucher(Guid id, [FromBody] UpdateVoucherDto dto)
    {
        var result = await _voucherService.UpdateVoucherAsync(id, dto);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpPut("{id}/toggle-status")]
    [Authorize(Roles = "admin,service_provider")]
    public async Task<IActionResult> ToggleVoucherStatus(Guid id)
    {
        var result = await _voucherService.ToggleVoucherStatusAsync(id);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(new { message = "Cập nhật trạng thái thành công", success = true });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteVoucher(Guid id)
    {
        var result = await _voucherService.DeleteVoucherAsync(id);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(new { message = "Xóa voucher thành công", success = true });
    }
}

public class ApplyVoucherDto
{
    public Guid OrderId { get; set; }
    public string VoucherCode { get; set; } = string.Empty;
}
