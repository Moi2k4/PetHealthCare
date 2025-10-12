using AutoMapper;
using PetCare.Application.Common;
using PetCare.Application.DTOs.Product;
using PetCare.Application.Services.Interfaces;
using PetCare.Domain.Entities;
using PetCare.Infrastructure.Repositories.Interfaces;

namespace PetCare.Application.Services.Implementations;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CartService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<IEnumerable<CartItemDto>>> GetCartItemsAsync(Guid userId)
    {
        try
        {
            var cartItemRepo = _unitOfWork.Repository<CartItem>();
            var cartItems = await cartItemRepo.FindAsync(c => c.UserId == userId);
            
            var cartItemDtos = new List<CartItemDto>();
            
            foreach (var item in cartItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    var dto = new CartItemDto
                    {
                        Id = item.Id,
                        ProductId = product.Id,
                        ProductName = product.ProductName,
                        Price = product.Price,
                        SalePrice = product.SalePrice,
                        Quantity = item.Quantity,
                        StockQuantity = product.StockQuantity,
                        ImageUrl = product.Images.FirstOrDefault()?.ImageUrl
                    };
                    cartItemDtos.Add(dto);
                }
            }

            return ServiceResult<IEnumerable<CartItemDto>>.SuccessResult(cartItemDtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<CartItemDto>>.FailureResult($"Error retrieving cart items: {ex.Message}");
        }
    }

    public async Task<ServiceResult<CartItemDto>> AddToCartAsync(Guid userId, AddToCartDto addToCartDto)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(addToCartDto.ProductId);
            if (product == null)
            {
                return ServiceResult<CartItemDto>.FailureResult("Product not found");
            }

            if (!product.IsActive)
            {
                return ServiceResult<CartItemDto>.FailureResult("Product is not available");
            }

            if (product.StockQuantity < addToCartDto.Quantity)
            {
                return ServiceResult<CartItemDto>.FailureResult($"Only {product.StockQuantity} items available in stock");
            }

            var cartItemRepo = _unitOfWork.Repository<CartItem>();
            var existingItem = await cartItemRepo.FirstOrDefaultAsync(c => 
                c.UserId == userId && c.ProductId == addToCartDto.ProductId);

            CartItem cartItem;
            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + addToCartDto.Quantity;
                if (product.StockQuantity < newQuantity)
                {
                    return ServiceResult<CartItemDto>.FailureResult($"Only {product.StockQuantity} items available in stock");
                }
                
                existingItem.Quantity = newQuantity;
                await cartItemRepo.UpdateAsync(existingItem);
                cartItem = existingItem;
            }
            else
            {
                cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = addToCartDto.ProductId,
                    Quantity = addToCartDto.Quantity
                };
                await cartItemRepo.AddAsync(cartItem);
            }

            await _unitOfWork.SaveChangesAsync();

            var dto = new CartItemDto
            {
                Id = cartItem.Id,
                ProductId = product.Id,
                ProductName = product.ProductName,
                Price = product.Price,
                SalePrice = product.SalePrice,
                Quantity = cartItem.Quantity,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.Images.FirstOrDefault()?.ImageUrl
            };

            return ServiceResult<CartItemDto>.SuccessResult(dto, "Product added to cart");
        }
        catch (Exception ex)
        {
            return ServiceResult<CartItemDto>.FailureResult($"Error adding to cart: {ex.Message}");
        }
    }

    public async Task<ServiceResult<CartItemDto>> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemDto updateDto)
    {
        try
        {
            var cartItemRepo = _unitOfWork.Repository<CartItem>();
            var cartItem = await cartItemRepo.FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);
            
            if (cartItem == null)
            {
                return ServiceResult<CartItemDto>.FailureResult("Cart item not found");
            }

            var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId);
            if (product == null)
            {
                return ServiceResult<CartItemDto>.FailureResult("Product not found");
            }

            if (product.StockQuantity < updateDto.Quantity)
            {
                return ServiceResult<CartItemDto>.FailureResult($"Only {product.StockQuantity} items available in stock");
            }

            cartItem.Quantity = updateDto.Quantity;
            await cartItemRepo.UpdateAsync(cartItem);
            await _unitOfWork.SaveChangesAsync();

            var dto = new CartItemDto
            {
                Id = cartItem.Id,
                ProductId = product.Id,
                ProductName = product.ProductName,
                Price = product.Price,
                SalePrice = product.SalePrice,
                Quantity = cartItem.Quantity,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.Images.FirstOrDefault()?.ImageUrl
            };

            return ServiceResult<CartItemDto>.SuccessResult(dto, "Cart item updated");
        }
        catch (Exception ex)
        {
            return ServiceResult<CartItemDto>.FailureResult($"Error updating cart item: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> RemoveFromCartAsync(Guid userId, Guid cartItemId)
    {
        try
        {
            var cartItemRepo = _unitOfWork.Repository<CartItem>();
            var cartItem = await cartItemRepo.FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);
            
            if (cartItem == null)
            {
                return ServiceResult<bool>.FailureResult("Cart item not found");
            }

            await cartItemRepo.DeleteAsync(cartItem);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true, "Item removed from cart");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error removing cart item: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> ClearCartAsync(Guid userId)
    {
        try
        {
            var cartItemRepo = _unitOfWork.Repository<CartItem>();
            var cartItems = await cartItemRepo.FindAsync(c => c.UserId == userId);
            
            await cartItemRepo.DeleteRangeAsync(cartItems);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true, "Cart cleared");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Error clearing cart: {ex.Message}");
        }
    }

    public async Task<ServiceResult<decimal>> GetCartTotalAsync(Guid userId)
    {
        try
        {
            var cartItemsResult = await GetCartItemsAsync(userId);
            if (!cartItemsResult.Success)
            {
                return ServiceResult<decimal>.FailureResult(cartItemsResult.Message);
            }

            var total = cartItemsResult.Data?.Sum(item => item.Subtotal) ?? 0;
            return ServiceResult<decimal>.SuccessResult(total);
        }
        catch (Exception ex)
        {
            return ServiceResult<decimal>.FailureResult($"Error calculating cart total: {ex.Message}");
        }
    }
}
