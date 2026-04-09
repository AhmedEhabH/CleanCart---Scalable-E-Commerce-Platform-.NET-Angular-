using ECommerce.Application.Cart.DTOs;
using ECommerce.Application.Cart.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly IApplicationDbContext _context;

    public CartService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CartDto>> GetCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        return Result<CartDto>.Success(MapToDto(cart));
    }

    public async Task<Result<CartDto>> AddItemAsync(Guid userId, AddCartItemRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive, cancellationToken);

        if (product == null)
            return Result<CartDto>.Failure("Product not found");

        if (request.Quantity <= 0)
            return Result<CartDto>.Failure("Quantity must be greater than zero");

        if (product.StockQuantity < request.Quantity)
            return Result<CartDto>.Failure($"Insufficient stock. Available: {product.StockQuantity}, Requested: {request.Quantity}");

        var cart = await GetOrCreateCartAsync(userId, cancellationToken);

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + request.Quantity);
            existingItem.SetUnitPrice(product.Price);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<CartDto>.Success(MapToDto(cart));
        }

        cart.AddItemWithPrice(product.Id, request.Quantity, product.Price);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<CartDto>.Success(MapToDto(cart));
    }

    public async Task<Result<CartDto>> UpdateItemQuantityAsync(Guid userId, Guid itemId, UpdateCartItemRequest request, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);

        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return Result<CartDto>.Failure("Cart item not found");

        if (request.Quantity <= 0)
        {
            cart.RemoveItem(itemId);
        }
        else
        {
            cart.UpdateItemQuantity(itemId, request.Quantity);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<CartDto>.Success(MapToDto(cart));
    }

    public async Task<Result> RemoveItemAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);

        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return Result.Failure("Cart item not found");

        cart.RemoveItem(itemId);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ClearCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        cart.Clear();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Cart> GetOrCreateCartAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive, cancellationToken);

        if (cart == null)
        {
            cart = Cart.CreateForUser(userId);
            _context.Carts.Add(cart);
        }

        return cart;
    }

    private static CartDto MapToDto(Cart cart)
    {
        var items = cart.Items.Select(i => new CartItemDto(
            Id: i.Id,
            ProductId: i.ProductId,
            ProductName: i.Product?.Name ?? "Unknown",
            ProductSlug: i.Product?.Slug ?? "",
            Quantity: i.Quantity,
            UnitPrice: i.UnitPrice,
            Total: i.Total,
            IsInStock: i.Product?.IsInStock ?? false
        )).ToList();

        return new CartDto(
            Id: cart.Id,
            Items: items.AsReadOnly(),
            TotalItems: cart.TotalItems,
            SubTotal: cart.SubTotal,
            IsEmpty: cart.IsEmpty
        );
    }
}
