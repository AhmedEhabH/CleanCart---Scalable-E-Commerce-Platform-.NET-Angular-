using ECommerce.Application.Cart.DTOs;
using ECommerce.Application.Common.Models;

namespace ECommerce.Application.Cart.Interfaces;

public interface ICartService
{
    Task<Result<CartDto>> GetCartAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<CartDto>> AddItemAsync(Guid userId, AddCartItemRequest request, CancellationToken cancellationToken = default);
    Task<Result<CartDto>> UpdateItemQuantityAsync(Guid userId, Guid itemId, UpdateCartItemRequest request, CancellationToken cancellationToken = default);
    Task<Result> RemoveItemAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default);
    Task<Result> ClearCartAsync(Guid userId, CancellationToken cancellationToken = default);
}
