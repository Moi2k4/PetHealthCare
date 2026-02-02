using AutoMapper;
using PetCare.Application.Common;
using PetCare.Application.DTOs.Payment;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Repositories.Interfaces;

namespace PetCare.Application.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<PaymentDto>> CreatePaymentAsync(CreatePaymentDto dto)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(dto.OrderId);
            if (order == null)
            {
                return ServiceResult<PaymentDto>.FailureResult("Order not found");
            }

            // Check if payment already exists for this order
            var paymentRepo = _unitOfWork.Repository<Payment>();
            var existingPayment = await paymentRepo.FirstOrDefaultAsync(p => p.OrderId == dto.OrderId);
            if (existingPayment != null)
            {
                return ServiceResult<PaymentDto>.FailureResult("Payment already exists for this order");
            }

            var payment = new Payment
            {
                OrderId = dto.OrderId,
                UserId = order.UserId,
                PaymentMethod = dto.PaymentMethod,
                PaymentStatus = "pending",
                Amount = dto.Amount
            };

            await paymentRepo.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            var paymentDto = _mapper.Map<PaymentDto>(payment);
            return ServiceResult<PaymentDto>.SuccessResult(paymentDto, "Payment created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<PaymentDto>.FailureResult($"Error creating payment: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PaymentUrlResponseDto>> GeneratePaymentUrlAsync(CreatePaymentDto dto)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(dto.OrderId);
            if (order == null)
            {
                return ServiceResult<PaymentUrlResponseDto>.FailureResult("Order not found");
            }

            // Create or get payment
            var paymentRepo = _unitOfWork.Repository<Payment>();
            var payment = await paymentRepo.FirstOrDefaultAsync(p => p.OrderId == dto.OrderId);
            
            if (payment == null)
            {
                payment = new Payment
                {
                    OrderId = dto.OrderId,
                    UserId = order.UserId,
                    PaymentMethod = dto.PaymentMethod,
                    PaymentStatus = "pending",
                    Amount = dto.Amount
                };
                await paymentRepo.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();
            }

            string paymentUrl;
            switch (dto.PaymentMethod.ToLower())
            {
                case "vnpay":
                    paymentUrl = GenerateVNPayUrl(payment, order, dto.ReturnUrl ?? "");
                    break;
                case "momo":
                    paymentUrl = GenerateMoMoUrl(payment, order, dto.ReturnUrl ?? "");
                    break;
                case "paypal":
                    paymentUrl = GeneratePayPalUrl(payment, order, dto.ReturnUrl ?? "");
                    break;
                case "cod":
                    paymentUrl = string.Empty; // Cash on delivery doesn't need a payment URL
                    break;
                default:
                    return ServiceResult<PaymentUrlResponseDto>.FailureResult("Unsupported payment method");
            }

            var response = new PaymentUrlResponseDto
            {
                PaymentUrl = paymentUrl,
                PaymentMethod = dto.PaymentMethod,
                PaymentId = payment.Id
            };

            return ServiceResult<PaymentUrlResponseDto>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            return ServiceResult<PaymentUrlResponseDto>.FailureResult($"Error generating payment URL: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PaymentDto>> ProcessPaymentCallbackAsync(PaymentCallbackDto dto)
    {
        try
        {
            var paymentRepo = _unitOfWork.Repository<Payment>();
            
            // Find payment by order ID from callback
            Guid orderId;
            if (!Guid.TryParse(dto.OrderId, out orderId))
            {
                return ServiceResult<PaymentDto>.FailureResult("Invalid order ID");
            }

            var payment = await paymentRepo.FirstOrDefaultAsync(p => p.OrderId == orderId);
            if (payment == null)
            {
                return ServiceResult<PaymentDto>.FailureResult("Payment not found");
            }

            // Update payment status based on callback
            payment.TransactionId = dto.TransactionId;
            payment.PaymentGatewayResponse = System.Text.Json.JsonSerializer.Serialize(dto.AdditionalData);
            
            // Map payment gateway status to our status
            if (dto.Status == "00" || dto.Status == "success" || dto.Status.ToLower() == "completed")
            {
                payment.PaymentStatus = "completed";
                payment.PaidAt = DateTime.UtcNow;

                // Update order payment status
                var order = await _unitOfWork.Orders.GetByIdAsync(payment.OrderId);
                if (order != null)
                {
                    order.PaymentStatus = "paid";
                    order.OrderStatus = "Confirmed";
                    await _unitOfWork.Orders.UpdateAsync(order);
                }
            }
            else
            {
                payment.PaymentStatus = "failed";
            }

            await paymentRepo.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            var paymentDto = _mapper.Map<PaymentDto>(payment);
            return ServiceResult<PaymentDto>.SuccessResult(paymentDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<PaymentDto>.FailureResult($"Error processing payment callback: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PaymentDto>> GetPaymentByIdAsync(Guid id)
    {
        try
        {
            var paymentRepo = _unitOfWork.Repository<Payment>();
            var payment = await paymentRepo.GetByIdAsync(id);
            
            if (payment == null)
            {
                return ServiceResult<PaymentDto>.FailureResult("Payment not found");
            }

            var paymentDto = _mapper.Map<PaymentDto>(payment);
            return ServiceResult<PaymentDto>.SuccessResult(paymentDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<PaymentDto>.FailureResult($"Error retrieving payment: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PaymentDto>> GetPaymentByOrderIdAsync(Guid orderId)
    {
        try
        {
            var paymentRepo = _unitOfWork.Repository<Payment>();
            var payment = await paymentRepo.FirstOrDefaultAsync(p => p.OrderId == orderId);
            
            if (payment == null)
            {
                return ServiceResult<PaymentDto>.FailureResult("Payment not found");
            }

            var paymentDto = _mapper.Map<PaymentDto>(payment);
            return ServiceResult<PaymentDto>.SuccessResult(paymentDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<PaymentDto>.FailureResult($"Error retrieving payment: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PagedResult<PaymentDto>>> GetUserPaymentsAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var paymentRepo = _unitOfWork.Repository<Payment>();
            var (payments, totalCount) = await paymentRepo.GetPagedAsync(
                pageNumber,
                pageSize,
                filter: p => p.UserId == userId,
                orderBy: q => q.OrderByDescending(p => p.CreatedAt));

            var paymentDtos = _mapper.Map<IEnumerable<PaymentDto>>(payments);

            var pagedResult = new PagedResult<PaymentDto>
            {
                Items = paymentDtos,
                TotalCount = totalCount,
                Page = pageNumber,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<PaymentDto>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            return ServiceResult<PagedResult<PaymentDto>>.FailureResult($"Error retrieving user payments: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> RefundPaymentAsync(RefundPaymentDto dto)
    {
        try
        {
            var paymentRepo = _unitOfWork.Repository<Payment>();
            var payment = await paymentRepo.GetByIdAsync(dto.PaymentId);
            
            if (payment == null)
            {
                return ServiceResult<bool>.FailureResult("Payment not found");
            }

            if (payment.PaymentStatus != "completed")
            {
                return ServiceResult<bool>.FailureResult("Only completed payments can be refunded");
            }

            var refundAmount = dto.RefundAmount ?? payment.Amount;
            if (refundAmount > payment.Amount)
            {
                return ServiceResult<bool>.FailureResult("Refund amount cannot exceed payment amount");
            }

            payment.PaymentStatus = "refunded";
            payment.RefundAmount = refundAmount;
            payment.RefundReason = dto.Reason;
            payment.RefundedAt = DateTime.UtcNow;

            await paymentRepo.UpdateAsync(payment);

            // Update order status
            var order = await _unitOfWork.Orders.GetByIdAsync(payment.OrderId);
            if (order != null)
            {
                order.PaymentStatus = "refunded";
                order.OrderStatus = "Cancelled";
                await _unitOfWork.Orders.UpdateAsync(order);
            }

            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true, "Payment refunded successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error refunding payment: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> UpdatePaymentStatusAsync(Guid paymentId, string status)
    {
        try
        {
            var paymentRepo = _unitOfWork.Repository<Payment>();
            var payment = await paymentRepo.GetByIdAsync(paymentId);
            
            if (payment == null)
            {
                return ServiceResult<bool>.FailureResult("Payment not found");
            }

            payment.PaymentStatus = status;
            
            if (status.ToLower() == "completed")
            {
                payment.PaidAt = DateTime.UtcNow;
            }

            await paymentRepo.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true, "Payment status updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error updating payment status: {ex.Message}");
        }
    }

    #region Payment Gateway URL Generators

    private string GenerateVNPayUrl(Payment payment, Order order, string returnUrl)
    {
        // TODO: Implement VNPay URL generation
        // This should use VNPay SDK or API to generate the payment URL
        // For now, returning a placeholder
        var vnpUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        var queryString = $"?vnp_TxnRef={order.OrderNumber}&vnp_Amount={payment.Amount * 100}&vnp_ReturnUrl={returnUrl}";
        return vnpUrl + queryString;
    }

    private string GenerateMoMoUrl(Payment payment, Order order, string returnUrl)
    {
        // TODO: Implement MoMo URL generation
        // This should use MoMo SDK or API to generate the payment URL
        return "https://test-payment.momo.vn/v2/gateway/api/create";
    }

    private string GeneratePayPalUrl(Payment payment, Order order, string returnUrl)
    {
        // TODO: Implement PayPal URL generation
        // This should use PayPal SDK or API to generate the payment URL
        return "https://www.sandbox.paypal.com/checkoutnow";
    }

    #endregion
}
