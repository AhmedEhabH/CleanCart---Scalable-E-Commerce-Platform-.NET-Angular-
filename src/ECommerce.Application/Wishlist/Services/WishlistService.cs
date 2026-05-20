using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Wishlist.DTOs;
using ECommerce.Application.Wishlist.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Wishlist.Services;

public class WishlistService : IWishlistService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<WishlistService> _logger;

    public WishlistService(IApplicationDbContext context, ILogger<WishlistService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<WishlistItemDto>>> GetUserWishlistAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var wishlist = await _context.Wishlists
                .Include(w => w.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p!.Images)
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            if (wishlist == null)
            {
                wishlist = Domain.Entities.Wishlist.Create(userId);
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var items = wishlist.Items.Select(i => new WishlistItemDto(
                i.Id,
                i.ProductId,
                i.Product?.Name ?? string.Empty,
                i.Product?.Price ?? 0,
                i.Product?.MainImageUrl,
                i.AddedAt
            )).ToList();

            return Result<List<WishlistItemDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving wishlist for user {UserId}", userId);
            return Result<List<WishlistItemDto>>.Failure("Failed to retrieve wishlist", "WISHLIST_GET_ERROR");
        }
    }

    public async Task<Result<bool>> ToggleWishlistItemAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            var productExists = await _context.Products.AnyAsync(p => p.Id == productId, cancellationToken);
            if (!productExists)
                return Result<bool>.Failure($"Product with ID '{productId}' was not found", "PRODUCT_NOT_FOUND");

            var wishlist = await _context.Wishlists
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            if (wishlist == null)
            {
                wishlist = Domain.Entities.Wishlist.Create(userId);
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var existingItem = wishlist.Items.FirstOrDefault(i => i.ProductId == productId);
            bool isAdded;

            if (existingItem != null)
            {
                _context.WishlistItems.Remove(existingItem);
                isAdded = false;
            }
            else
            {
                var item = Domain.Entities.WishlistItem.Create(wishlist.Id, productId);
                _context.WishlistItems.Add(item);
                isAdded = true;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(isAdded);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling wishlist item for user {UserId}, product {ProductId}", userId, productId);
            return Result<bool>.Failure($"DB Error: {ex.InnerException?.Message ?? ex.Message}", "WISHLIST_TOGGLE_ERROR");
        }
    }

    public async Task<Result<List<WishlistItemDto>>> SyncWishlistAsync(Guid userId, List<Guid>? localProductIds, CancellationToken cancellationToken = default)
    {
        try
        {
            if (localProductIds == null || localProductIds.Count == 0)
                return Result<List<WishlistItemDto>>.Success(new List<WishlistItemDto>());

            var wishlist = await _context.Wishlists
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            if (wishlist == null)
            {
                wishlist = Domain.Entities.Wishlist.Create(userId);
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var existingProductIds = wishlist.Items.Select(i => i.ProductId).ToHashSet();
            var productsToAdd = localProductIds.Except(existingProductIds).ToList();

            if (productsToAdd.Count > 0)
            {
                var validProductIds = await _context.Products
                    .Where(p => productsToAdd.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync(cancellationToken);

                foreach (var productId in validProductIds)
                {
                    var item = Domain.Entities.WishlistItem.Create(wishlist.Id, productId);
                    _context.WishlistItems.Add(item);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }

            var updatedWishlist = await _context.Wishlists
                .Include(w => w.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p!.Images)
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            var items = updatedWishlist!.Items.Select(i => new WishlistItemDto(
                i.Id,
                i.ProductId,
                i.Product?.Name ?? string.Empty,
                i.Product?.Price ?? 0,
                i.Product?.MainImageUrl,
                i.AddedAt
            )).ToList();

            return Result<List<WishlistItemDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing wishlist for user {UserId}", userId);
            return Result<List<WishlistItemDto>>.Failure("Failed to sync wishlist", "WISHLIST_SYNC_ERROR");
        }
    }
}
