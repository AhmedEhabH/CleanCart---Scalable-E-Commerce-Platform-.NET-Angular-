using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Wishlist.DTOs;
using ECommerce.Application.Wishlist.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Wishlist.Services;

public class WishlistService : IWishlistService
{
    private readonly IApplicationDbContext _context;

    public WishlistService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<WishlistItemDto>>> GetUserWishlistAsync(Guid userId, CancellationToken cancellationToken = default)
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

    public async Task<Result<bool>> ToggleWishlistItemAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        var wishlist = await _context.Wishlists
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wishlist == null)
        {
            wishlist = Domain.Entities.Wishlist.Create(userId);
            _context.Wishlists.Add(wishlist);
        }

        if (wishlist.HasProduct(productId))
        {
            wishlist.RemoveItemByProduct(productId);
        }
        else
        {
            var productExists = await _context.Products.AnyAsync(p => p.Id == productId, cancellationToken);
            if (!productExists)
                return Result<bool>.Failure("Product not found", "PRODUCT_NOT_FOUND");

            wishlist.AddItem(productId);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(!wishlist.HasProduct(productId));
    }

    public async Task<Result<List<WishlistItemDto>>> SyncWishlistAsync(Guid userId, List<Guid> localProductIds, CancellationToken cancellationToken = default)
    {
        var wishlist = await _context.Wishlists
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wishlist == null)
        {
            wishlist = Domain.Entities.Wishlist.Create(userId);
            _context.Wishlists.Add(wishlist);
        }

        var existingProductIds = wishlist.Items.Select(i => i.ProductId).ToHashSet();

        foreach (var productId in localProductIds.Except(existingProductIds))
        {
            var productExists = await _context.Products.AnyAsync(p => p.Id == productId, cancellationToken);
            if (productExists)
            {
                wishlist.AddItem(productId);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

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
}