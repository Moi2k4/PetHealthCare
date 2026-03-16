using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCare.Application.DTOs.Voucher;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Data;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly PetCareDbContext _context;

    public AdminDashboardController(PetCareDbContext context)
    {
        _context = context;
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueOverview()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthStart = monthStart.AddMonths(1);

        var paidOrdersQuery = _context.Orders
            .AsNoTracking()
            .Where(order =>
                (order.PaymentStatus != null &&
                 (order.PaymentStatus.ToLower() == "paid" || order.PaymentStatus.ToLower() == "completed")) ||
                (order.PaymentMethod != null && order.PaymentMethod.ToLower() == "cod" &&
                 order.OrderStatus != null &&
                 (order.OrderStatus.ToLower() == "completed" || order.OrderStatus.ToLower() == "delivered")));

        var totalRevenue = await paidOrdersQuery.SumAsync(order => (decimal?)order.FinalAmount) ?? 0m;
        var paidOrders = await paidOrdersQuery.CountAsync();
        var paidRevenueThisMonth = await paidOrdersQuery
            .Where(order => order.OrderedAt >= monthStart && order.OrderedAt < nextMonthStart)
            .SumAsync(order => (decimal?)order.FinalAmount) ?? 0m;

        var totalOrders = await _context.Orders.AsNoTracking().CountAsync();

        return Ok(new
        {
            success = true,
            message = "Revenue overview retrieved successfully",
            data = new
            {
                totalRevenue,
                paidRevenueThisMonth,
                totalOrders,
                paidOrders,
                generatedAt = now
            }
        });
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthStart = monthStart.AddMonths(1);

        var paidOrdersQuery = _context.Orders
            .AsNoTracking()
            .Where(order =>
                (order.PaymentStatus != null &&
                 (order.PaymentStatus.ToLower() == "paid" || order.PaymentStatus.ToLower() == "completed")) ||
                (order.PaymentMethod != null && order.PaymentMethod.ToLower() == "cod" &&
                 order.OrderStatus != null &&
                 (order.OrderStatus.ToLower() == "completed" || order.OrderStatus.ToLower() == "delivered")));

        var totalUsers = await _context.Users.AsNoTracking().CountAsync();
        var activeUsers = await _context.Users.AsNoTracking().CountAsync(u => u.IsActive);
        var newUsersThisMonth = await _context.Users.AsNoTracking()
            .CountAsync(u => u.CreatedAt >= monthStart && u.CreatedAt < nextMonthStart);

        var totalProducts = await _context.Products.AsNoTracking().CountAsync();
        var activeProducts = await _context.Products.AsNoTracking().CountAsync(p => p.IsActive);
        var lowStockProducts = await _context.Products.AsNoTracking().CountAsync(p => p.IsActive && p.StockQuantity <= 10);

        var totalBlogs = await _context.BlogPosts.AsNoTracking().CountAsync();
        var publishedBlogs = await _context.BlogPosts.AsNoTracking()
            .CountAsync(b => b.Status != null && b.Status.ToLower() == "published");
        var totalBlogViews = await _context.BlogPosts.AsNoTracking().SumAsync(b => (int?)b.ViewCount) ?? 0;

        var totalRevenue = await paidOrdersQuery.SumAsync(order => (decimal?)order.FinalAmount) ?? 0m;
        var paidOrders = await paidOrdersQuery.CountAsync();
        var paidRevenueThisMonth = await paidOrdersQuery
            .Where(order => order.OrderedAt >= monthStart && order.OrderedAt < nextMonthStart)
            .SumAsync(order => (decimal?)order.FinalAmount) ?? 0m;
        var totalOrders = await _context.Orders.AsNoTracking().CountAsync();

        var totalVouchers = await _context.Vouchers.AsNoTracking().CountAsync();
        var activeVouchers = await _context.Vouchers.AsNoTracking().CountAsync(v => v.IsActive);
        var expiringVouchers = await _context.Vouchers.AsNoTracking()
            .CountAsync(v => v.IsActive && v.ValidTo >= now && v.ValidTo <= now.AddDays(7));

        var recentUsers = await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                RoleName = u.Role != null ? u.Role.RoleName : null,
                u.IsActive,
                u.CreatedAt
            })
            .ToListAsync();

        var topVouchers = await _context.Vouchers
            .AsNoTracking()
            .OrderByDescending(v => v.UsedCount)
            .ThenByDescending(v => v.CreatedAt)
            .Take(5)
            .Select(v => new
            {
                v.Id,
                v.Code,
                v.Name,
                v.DiscountType,
                v.DiscountValue,
                v.IsActive,
                v.ValidTo,
                v.UsedCount,
                v.UsageLimit,
                RemainingUsage = v.UsageLimit.HasValue ? Math.Max(0, v.UsageLimit.Value - v.UsedCount) : (int?)null
            })
            .ToListAsync();

        var lowStockProductsList = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.StockQuantity <= 10)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.ProductName)
            .Take(5)
            .Select(p => new
            {
                p.Id,
                p.ProductName,
                p.StockQuantity,
                CategoryName = p.Category != null ? p.Category.CategoryName : null,
                p.IsActive,
                p.Price,
                p.SalePrice
            })
            .ToListAsync();

        var topPosts = await _context.BlogPosts
            .AsNoTracking()
            .Include(b => b.Author)
            .Include(b => b.Category)
            .OrderByDescending(b => b.ViewCount + (b.Likes.Count * 3))
            .ThenByDescending(b => b.CreatedAt)
            .Take(4)
            .Select(b => new
            {
                b.Id,
                b.Title,
                b.Status,
                b.ViewCount,
                LikeCount = b.Likes.Count,
                CategoryName = b.Category != null ? b.Category.CategoryName : null,
                AuthorName = b.Author != null ? b.Author.FullName : null,
                b.CreatedAt
            })
            .ToListAsync();

        var latestPosts = await _context.BlogPosts
            .AsNoTracking()
            .Include(b => b.Author)
            .Include(b => b.Category)
            .OrderByDescending(b => b.CreatedAt)
            .Take(4)
            .Select(b => new
            {
                b.Id,
                b.Title,
                b.Status,
                b.ViewCount,
                LikeCount = b.Likes.Count,
                CategoryName = b.Category != null ? b.Category.CategoryName : null,
                AuthorName = b.Author != null ? b.Author.FullName : null,
                b.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            message = "Admin overview retrieved successfully",
            data = new
            {
                totals = new
                {
                    users = totalUsers,
                    activeUsers,
                    newUsersThisMonth,
                    products = totalProducts,
                    activeProducts,
                    lowStockProducts,
                    blogs = totalBlogs,
                    publishedBlogs,
                    totalBlogViews,
                    totalRevenue,
                    revenueThisMonth = paidRevenueThisMonth,
                    totalOrders,
                    paidOrders,
                    totalVouchers,
                    activeVouchers,
                    expiringVouchers
                },
                recentUsers,
                lowStockProducts = lowStockProductsList,
                topPosts,
                latestPosts,
                topVouchers,
                generatedAt = now
            }
        });
    }

    [HttpGet("vouchers")]
    public async Task<IActionResult> GetVouchers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        if (page < 1 || pageSize < 1 || pageSize > 200)
        {
            return BadRequest(new { success = false, message = "Invalid pagination parameters" });
        }

        var query = _context.Vouchers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToUpper();
            query = query.Where(v =>
                v.Code.ToUpper().Contains(keyword)
                || v.Name.ToUpper().Contains(keyword));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new VoucherDto
            {
                Id = v.Id,
                Code = v.Code,
                Name = v.Name,
                Description = v.Description,
                DiscountType = v.DiscountType,
                DiscountValue = v.DiscountValue,
                MinimumOrderAmount = v.MinimumOrderAmount,
                MaximumDiscountAmount = v.MaximumDiscountAmount,
                UsageLimit = v.UsageLimit,
                UsedCount = v.UsedCount,
                ValidFrom = v.ValidFrom,
                ValidTo = v.ValidTo,
                IsActive = v.IsActive
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            message = "Vouchers retrieved successfully",
            data = new
            {
                items,
                totalCount,
                page,
                pageSize
            }
        });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 200)
        {
            return BadRequest(new { success = false, message = "Invalid pagination parameters" });
        }

        var query = _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .OrderByDescending(u => u.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                RoleName = u.Role != null ? u.Role.RoleName : null,
                u.IsActive,
                u.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            message = "Users retrieved successfully",
            data = new
            {
                items,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            }
        });
    }

    [HttpPost("vouchers")]
    public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new { success = false, message = "Voucher code and name are required" });
        }

        var normalizedCode = dto.Code.Trim().ToUpperInvariant();
        var duplicate = await _context.Vouchers.AnyAsync(v => v.Code.ToUpper() == normalizedCode);
        if (duplicate)
        {
            return BadRequest(new { success = false, message = "Voucher code already exists" });
        }

        if (dto.ValidTo <= dto.ValidFrom)
        {
            return BadRequest(new { success = false, message = "ValidTo must be later than ValidFrom" });
        }

        if (dto.DiscountValue <= 0)
        {
            return BadRequest(new { success = false, message = "Discount value must be greater than 0" });
        }

        var discountType = (dto.DiscountType ?? "percentage").Trim().ToLowerInvariant();
        if (discountType != "percentage" && discountType != "fixed")
        {
            return BadRequest(new { success = false, message = "DiscountType must be 'percentage' or 'fixed'" });
        }

        var now = DateTime.UtcNow;
        var voucher = new Voucher
        {
            Code = normalizedCode,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            DiscountType = discountType,
            DiscountValue = dto.DiscountValue,
            MinimumOrderAmount = dto.MinimumOrderAmount,
            MaximumDiscountAmount = dto.MaximumDiscountAmount,
            UsageLimit = dto.UsageLimit,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.Vouchers.AddAsync(voucher);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Voucher created successfully",
            data = voucher.Id
        });
    }

    [HttpPut("vouchers/{voucherId:guid}")]
    public async Task<IActionResult> UpdateVoucher(Guid voucherId, [FromBody] AdminUpdateVoucherDto dto)
    {
        var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Id == voucherId);
        if (voucher == null)
        {
            return NotFound(new { success = false, message = "Voucher not found" });
        }

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            voucher.Name = dto.Name.Trim();
        }

        if (dto.Description != null)
        {
            voucher.Description = dto.Description.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.DiscountType))
        {
            var discountType = dto.DiscountType.Trim().ToLowerInvariant();
            if (discountType != "percentage" && discountType != "fixed")
            {
                return BadRequest(new { success = false, message = "DiscountType must be 'percentage' or 'fixed'" });
            }

            voucher.DiscountType = discountType;
        }

        if (dto.DiscountValue.HasValue)
        {
            if (dto.DiscountValue.Value <= 0)
            {
                return BadRequest(new { success = false, message = "Discount value must be greater than 0" });
            }

            voucher.DiscountValue = dto.DiscountValue.Value;
        }

        if (dto.MinimumOrderAmount.HasValue)
        {
            voucher.MinimumOrderAmount = dto.MinimumOrderAmount.Value;
        }

        if (dto.MaximumDiscountAmount.HasValue)
        {
            voucher.MaximumDiscountAmount = dto.MaximumDiscountAmount.Value;
        }

        if (dto.UsageLimit.HasValue)
        {
            voucher.UsageLimit = dto.UsageLimit.Value;
        }

        if (dto.ValidFrom.HasValue)
        {
            voucher.ValidFrom = dto.ValidFrom.Value;
        }

        if (dto.ValidTo.HasValue)
        {
            voucher.ValidTo = dto.ValidTo.Value;
        }

        if (voucher.ValidTo <= voucher.ValidFrom)
        {
            return BadRequest(new { success = false, message = "ValidTo must be later than ValidFrom" });
        }

        if (dto.IsActive.HasValue)
        {
            voucher.IsActive = dto.IsActive.Value;
        }

        voucher.UpdatedAt = DateTime.UtcNow;

        _context.Vouchers.Update(voucher);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Voucher updated successfully" });
    }

    [HttpPatch("vouchers/{voucherId:guid}/toggle")]
    public async Task<IActionResult> ToggleVoucher(Guid voucherId)
    {
        var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Id == voucherId);
        if (voucher == null)
        {
            return NotFound(new { success = false, message = "Voucher not found" });
        }

        voucher.IsActive = !voucher.IsActive;
        voucher.UpdatedAt = DateTime.UtcNow;
        _context.Vouchers.Update(voucher);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = voucher.IsActive ? "Voucher activated" : "Voucher deactivated"
        });
    }

    public class AdminUpdateVoucherDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public decimal? MaximumDiscountAmount { get; set; }
        public int? UsageLimit { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool? IsActive { get; set; }
    }
}