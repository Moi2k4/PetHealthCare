namespace PetCare.Application.Services.Interfaces;

using PetCare.Application.Common;
using PetCare.Application.DTOs.Payment;

public interface IPaymentService
{
    Task<ServiceResult<PaymentDto>> CreatePaymentAsync(CreatePaymentDto dto);
    Task<ServiceResult<PaymentUrlResponseDto>> GeneratePaymentUrlAsync(CreatePaymentDto dto);
    Task<ServiceResult<PaymentDto>> ProcessPaymentCallbackAsync(PaymentCallbackDto dto);
    Task<ServiceResult<PaymentDto>> GetPaymentByIdAsync(Guid id);
    Task<ServiceResult<PaymentDto>> GetPaymentByOrderIdAsync(Guid orderId);
    Task<ServiceResult<PagedResult<PaymentDto>>> GetUserPaymentsAsync(Guid userId, int pageNumber = 1, int pageSize = 10);
    Task<ServiceResult<bool>> RefundPaymentAsync(RefundPaymentDto dto);
    Task<ServiceResult<bool>> UpdatePaymentStatusAsync(Guid paymentId, string status);
}
