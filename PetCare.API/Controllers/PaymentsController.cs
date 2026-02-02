namespace PetCare.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.DTOs.Payment;
using PetCare.Application.Services.Interfaces;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
    {
        var result = await _paymentService.CreatePaymentAsync(dto);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpPost("generate-url")]
    [Authorize]
    public async Task<IActionResult> GeneratePaymentUrl([FromBody] CreatePaymentDto dto)
    {
        var result = await _paymentService.GeneratePaymentUrlAsync(dto);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpPost("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> PaymentCallback([FromBody] PaymentCallbackDto dto)
    {
        var result = await _paymentService.ProcessPaymentCallbackAsync(dto);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpGet("vnpay-return")]
    [AllowAnonymous]
    public async Task<IActionResult> VNPayReturn([FromQuery] Dictionary<string, string> queryParams)
    {
        try
        {
            var callbackDto = new PaymentCallbackDto
            {
                PaymentMethod = "vnpay",
                TransactionId = queryParams.GetValueOrDefault("vnp_TransactionNo", ""),
                Status = queryParams.GetValueOrDefault("vnp_ResponseCode", ""),
                Amount = decimal.Parse(queryParams.GetValueOrDefault("vnp_Amount", "0")) / 100,
                OrderId = queryParams.GetValueOrDefault("vnp_TxnRef", ""),
                AdditionalData = queryParams
            };

            var result = await _paymentService.ProcessPaymentCallbackAsync(callbackDto);
            
            if (result.Success && callbackDto.Status == "00")
            {
                return Redirect($"/payment/success?orderId={callbackDto.OrderId}");
            }
            else
            {
                return Redirect($"/payment/failed?orderId={callbackDto.OrderId}");
            }
        }
        catch (Exception ex)
        {
            return Redirect($"/payment/error?message={ex.Message}");
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetPaymentById(Guid id)
    {
        var result = await _paymentService.GetPaymentByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpGet("order/{orderId}")]
    [Authorize]
    public async Task<IActionResult> GetPaymentByOrderId(Guid orderId)
    {
        var result = await _paymentService.GetPaymentByOrderIdAsync(orderId);
        if (!result.Success)
        {
            return NotFound(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpGet("user/my-payments")]
    [Authorize]
    public async Task<IActionResult> GetMyPayments([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user token" });
        }

        var result = await _paymentService.GetUserPaymentsAsync(userId, pageNumber, pageSize);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(result.Data);
    }

    [HttpPost("refund")]
    [Authorize(Roles = "admin,service_provider")]
    public async Task<IActionResult> RefundPayment([FromBody] RefundPaymentDto dto)
    {
        var result = await _paymentService.RefundPaymentAsync(dto);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(new { message = "Hoàn tiền thành công", success = true });
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "admin,service_provider")]
    public async Task<IActionResult> UpdatePaymentStatus(Guid id, [FromBody] UpdatePaymentStatusDto dto)
    {
        var result = await _paymentService.UpdatePaymentStatusAsync(id, dto.Status);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return Ok(new { message = "Cập nhật trạng thái thành công", success = true });
    }
}

public class UpdatePaymentStatusDto
{
    public string Status { get; set; } = string.Empty;
}
