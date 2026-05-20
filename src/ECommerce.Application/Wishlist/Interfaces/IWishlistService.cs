using ECommerce.Application.Common.Models;
using ECommerce.Application.Wishlist.DTOs;

namespace ECommerce.Application.Wishlist.Interfaces;

public interface IWishlistService
{
    Task<Result<List<WishlistItemDto>>> GetUserWishlistAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<bool>> ToggleWishlistItemAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    Task<Result<List<WishlistItemDto>>> SyncWishlistAsync(Guid userId, List<Guid> localProductIds, CancellationToken cancellationToken = default);
}