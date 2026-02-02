namespace PetCare.Application.DTOs.Voucher;

public class VoucherDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired => DateTime.UtcNow > ValidTo;
    public bool IsAvailable => IsActive && !IsExpired && (UsageLimit == null || UsedCount < UsageLimit);
}

public class CreateVoucherDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = "percentage";
    public decimal DiscountValue { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}

public class UpdateVoucherDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool? IsActive { get; set; }
}

public class ValidateVoucherDto
{
    public string Code { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
}

public class VoucherValidationResultDto
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public VoucherDto? Voucher { get; set; }
}
