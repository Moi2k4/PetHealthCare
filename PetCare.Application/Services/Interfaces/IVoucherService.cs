namespace PetCare.Application.Services.Interfaces;

using PetCare.Application.Common;
using PetCare.Application.DTOs.Voucher;

public interface IVoucherService
{
    Task<ServiceResult<VoucherDto>> CreateVoucherAsync(CreateVoucherDto dto);
    Task<ServiceResult<VoucherDto>> GetVoucherByIdAsync(Guid id);
    Task<ServiceResult<VoucherDto>> GetVoucherByCodeAsync(string code);
    Task<ServiceResult<PagedResult<VoucherDto>>> GetAllVouchersAsync(int pageNumber = 1, int pageSize = 10, bool? isActive = null);
    Task<ServiceResult<VoucherDto>> UpdateVoucherAsync(Guid id, UpdateVoucherDto dto);
    Task<ServiceResult<bool>> DeleteVoucherAsync(Guid id);
    Task<ServiceResult<bool>> ToggleVoucherStatusAsync(Guid id);
    Task<ServiceResult<VoucherValidationResultDto>> ValidateVoucherAsync(ValidateVoucherDto dto, Guid userId);
    Task<ServiceResult<bool>> ApplyVoucherToOrderAsync(Guid orderId, string voucherCode, Guid userId);
    Task<ServiceResult<List<VoucherDto>>> GetAvailableVouchersForUserAsync(Guid userId);
}
