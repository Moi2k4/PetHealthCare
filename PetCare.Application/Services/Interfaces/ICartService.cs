using PetCare.Application.Common;
using PetCare.Application.DTOs.Product;

namespace PetCare.Application.Services.Interfaces;

public interface ICartService
{
    Task<ServiceResult<IEnumerable<CartItemDto>>> GetCartItemsAsync(Guid userId);
    Task<ServiceResult<CartItemDto>> AddToCartAsync(Guid userId, AddToCartDto addToCartDto);
    Task<ServiceResult<CartItemDto>> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemDto updateDto);
    Task<ServiceResult<bool>> RemoveFromCartAsync(Guid userId, Guid cartItemId);
    Task<ServiceResult<bool>> ClearCartAsync(Guid userId);
    Task<ServiceResult<decimal>> GetCartTotalAsync(Guid userId);
}
