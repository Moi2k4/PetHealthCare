using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
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
    private readonly PayOSClient? _payOS;
    private readonly bool _payOsConfigured;
    private readonly string _fallbackReturnUrl;
    private readonly string _fallbackCancelUrl;

    public CheckoutController(PetCareDbContext context, IConfiguration configuration)
    {
        _context = context;

        var clientId = GetFirstNonEmpty(
            configuration["PayOS:ClientId"],
            Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID"));

        var apiKey = GetFirstNonEmpty(
            configuration["PayOS:ApiKey"],
            Environment.GetEnvironmentVariable("PAYOS_API_KEY"));

        var checksumKey = GetFirstNonEmpty(
            configuration["PayOS:ChecksumKey"],
            Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY"));

        _payOsConfigured = !string.IsNullOrWhiteSpace(clientId)
            && !string.IsNullOrWhiteSpace(apiKey)
            && !string.IsNullOrWhiteSpace(checksumKey);

        if (_payOsConfigured)
        {
            _payOS = new PayOSClient(clientId!, apiKey!, checksumKey!);
        }

        _fallbackReturnUrl = configuration["PayOS:CheckoutReturnUrl"]
            ?? configuration["PayOS:ReturnUrl"]
            ?? "https://pettsuba.live/thanh-toan/thanh-cong";

        _fallbackCancelUrl = configuration["PayOS:CheckoutCancelUrl"]
            ?? configuration["PayOS:CancelUrl"]
            ?? "https://pettsuba.live/thanh-toan";
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
        if (paymentMethod != "cod" && paymentMethod != "payos")
        {
            return BadRequest(new
            {
                success = false,
                message = "Unsupported payment method"
            });
        }

        if (paymentMethod == "payos" && (!_payOsConfigured || _payOS == null))
        {
            return BadRequest(new
            {
                success = false,
                message = "PayOS is not configured on server"
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

        if (paymentMethod == "payos" && finalAmount <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Final amount must be greater than 0 for PayOS payment"
            });
        }

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
            PaymentStatus = paymentMethod == "cod" ? "pending" : "unpaid",
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

        long? orderCode = null;
        string? paymentUrl = null;

        if (paymentMethod == "payos")
        {
            orderCode = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var (returnUrl, cancelUrl) = BuildClientReturnUrls(dto.ReturnBaseUrl);
            var amountVnd = Math.Max(1, decimal.ToInt32(decimal.Ceiling(finalAmount)));
            var payOsDescription = BuildPayOsDescription(order.OrderNumber);

            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode.Value,
                Amount = amountVnd,
                Description = payOsDescription,
                ReturnUrl = $"{returnUrl}?orderNumber={Uri.EscapeDataString(order.OrderNumber)}&amount={finalAmount}&method=payos",
                CancelUrl = cancelUrl,
                Items = cartItems.Select(ci => new PaymentLinkItem
                {
                    Name = ci.Product.ProductName,
                    Quantity = ci.Quantity,
                    Price = Math.Max(1, decimal.ToInt32(decimal.Ceiling(ci.Product.SalePrice ?? ci.Product.Price)))
                }).ToList()
            };

            try
            {
                var link = await _payOS!.PaymentRequests.CreateAsync(paymentRequest);
                paymentUrl = link.CheckoutUrl;
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"PayOS error: {ex.Message}"
                });
            }

            var payment = new Payment
            {
                OrderId = order.Id,
                UserId = userId,
                PaymentMethod = "payos",
                PaymentStatus = "pending",
                Amount = finalAmount,
                TransactionId = orderCode.ToString(),
                CreatedAt = now,
                UpdatedAt = now
            };

            await _context.Payments.AddAsync(payment);
        }
        else
        {
            var payment = new Payment
            {
                OrderId = order.Id,
                UserId = userId,
                PaymentMethod = "cod",
                PaymentStatus = "pending",
                Amount = finalAmount,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _context.Payments.AddAsync(payment);
        }

        _context.CartItems.RemoveRange(cartItems);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = $"Checkout save failed: {ex.GetBaseException().Message}"
            });
        }

        return Ok(new
        {
            success = true,
            message = paymentMethod == "payos" ? "Payment link created" : "Order placed successfully",
            data = new
            {
                order.Id,
                order.OrderNumber,
                order.FinalAmount,
                order.PaymentMethod,
                order.PaymentStatus,
                order.OrderStatus,
                order.OrderedAt,
                paymentUrl,
                orderCode
            }
        });
    }

    [AllowAnonymous]
    [HttpPost("payos-webhook")]
    public async Task<IActionResult> PayOsWebhook([FromBody] PayOsWebhookRequest webhook)
    {
        if (!_payOsConfigured || _payOS == null)
        {
            return Ok(new { success = false, message = "PayOS not configured" });
        }

        try
        {
            var webhookData = webhook.Data;

            var sdkWebhook = new Webhook
            {
                Code = webhook.Code,
                Description = webhook.Desc,
                Success = webhook.Success,
                Signature = webhook.Signature,
                Data = new WebhookData
                {
                    OrderCode = webhookData?.OrderCode ?? 0,
                    Amount = webhookData?.Amount ?? 0,
                    Description = webhookData?.Description ?? string.Empty,
                    AccountNumber = webhookData?.AccountNumber ?? string.Empty,
                    Reference = webhookData?.Reference ?? string.Empty,
                    TransactionDateTime = webhookData?.TransactionDateTime ?? string.Empty,
                    Currency = webhookData?.Currency ?? string.Empty,
                    PaymentLinkId = webhookData?.PaymentLinkId ?? string.Empty,
                    Code = webhookData?.Code ?? string.Empty,
                    CounterAccountBankId = webhookData?.CounterAccountBankId ?? string.Empty,
                    CounterAccountBankName = webhookData?.CounterAccountBankName ?? string.Empty,
                    CounterAccountName = webhookData?.CounterAccountName ?? string.Empty,
                    CounterAccountNumber = webhookData?.CounterAccountNumber ?? string.Empty,
                    VirtualAccountName = webhookData?.VirtualAccountName ?? string.Empty,
                    VirtualAccountNumber = webhookData?.VirtualAccountNumber ?? string.Empty
                }
            };

            var verified = await _payOS.Webhooks.VerifyAsync(sdkWebhook);
            var code = verified.OrderCode.ToString();

            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.TransactionId == code && p.PaymentMethod == "payos");

            if (payment == null)
            {
                return Ok(new { success = false, message = "Payment not found" });
            }

            payment.PaymentGatewayResponse = JsonSerializer.Serialize(webhook);
            payment.UpdatedAt = DateTime.UtcNow;

            if (verified.Code == "00" && webhook.Success)
            {
                payment.PaymentStatus = "completed";
                payment.PaidAt = DateTime.UtcNow;
                payment.Order.PaymentStatus = "paid";
                payment.Order.OrderStatus = "confirmed";
                payment.Order.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                payment.PaymentStatus = "failed";
                payment.Order.PaymentStatus = "failed";
                payment.Order.OrderStatus = "cancelled";
                payment.Order.UpdatedAt = DateTime.UtcNow;
            }

            _context.Payments.Update(payment);
            _context.Orders.Update(payment.Order);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return Ok(new { success = false, message = ex.Message });
        }
    }

    private (string returnUrl, string cancelUrl) BuildClientReturnUrls(string? returnBaseUrl)
    {
        if (Uri.TryCreate(returnBaseUrl, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            var origin = $"{uri.Scheme}://{uri.Host}{(uri.IsDefaultPort ? string.Empty : $":" + uri.Port)}";
            return ($"{origin}/thanh-toan/thanh-cong", $"{origin}/thanh-toan");
        }

        return (_fallbackReturnUrl, _fallbackCancelUrl);
    }

    private static string? GetFirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static string BuildPayOsDescription(string orderNumber)
    {
        // PayOS requires max 25 characters for description.
        var compactOrder = orderNumber.Replace("ORD", "DH", StringComparison.OrdinalIgnoreCase);
        var raw = $"DH {compactOrder}";
        return raw.Length <= 25 ? raw : raw[..25];
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

    public class PayOsWebhookRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Signature { get; set; } = string.Empty;
        public PayOsWebhookData? Data { get; set; }
    }

    public class PayOsWebhookData
    {
        public long OrderCode { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? AccountNumber { get; set; }
        public string? Reference { get; set; }
        public string? TransactionDateTime { get; set; }
        public string? Currency { get; set; }
        public string PaymentLinkId { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? CounterAccountBankId { get; set; }
        public string? CounterAccountBankName { get; set; }
        public string? CounterAccountName { get; set; }
        public string? CounterAccountNumber { get; set; }
        public string? VirtualAccountName { get; set; }
        public string? VirtualAccountNumber { get; set; }
    }
}
