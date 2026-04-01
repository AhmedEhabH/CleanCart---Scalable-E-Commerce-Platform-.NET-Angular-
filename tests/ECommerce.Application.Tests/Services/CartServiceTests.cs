using ECommerce.Application.Cart.DTOs;
using ECommerce.Application.Common.Models;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace ECommerce.Application.Tests.Services;

public class CartServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CartService _sut;

    public CartServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new ApplicationDbContext(options);
        _sut = new CartService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetCartAsync Tests

    [Fact]
    public async Task GetCartAsync_ShouldReturnEmptyCart_WhenNoCartExists()
    {
        var userId = Guid.NewGuid();

        var result = await _sut.GetCartAsync(userId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.IsEmpty.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    #endregion

    #region AddItemAsync Tests

    [Fact]
    public async Task AddItemAsync_ShouldReturnFailure_WhenProductDoesNotExist()
    {
        var userId = Guid.NewGuid();

        var result = await _sut.AddItemAsync(userId, new AddCartItemRequest(Guid.NewGuid(), 1));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Product not found");
    }

    [Fact]
    public async Task AddItemAsync_ShouldReturnFailure_WhenProductIsInactive()
    {
        var userId = Guid.NewGuid();
        var product = CreateProduct("Inactive Product", 25.00m);
        product.Deactivate();
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _sut.AddItemAsync(userId, new AddCartItemRequest(product.Id, 1));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Product not found");
    }

    [Fact]
    public async Task AddItemAsync_ShouldReturnFailure_WhenQuantityIsZero()
    {
        var userId = Guid.NewGuid();
        var product = CreateProduct("Test Product", 25.00m);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _sut.AddItemAsync(userId, new AddCartItemRequest(product.Id, 0));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Quantity must be greater than zero");
    }

    [Fact]
    public async Task AddItemAsync_ShouldReturnFailure_WhenInsufficientStock()
    {
        var userId = Guid.NewGuid();
        var product = CreateProduct("Test Product", 25.00m, stockQuantity: 5);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _sut.AddItemAsync(userId, new AddCartItemRequest(product.Id, 10));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Insufficient stock");
    }

    #endregion

    #region UpdateItemQuantityAsync Tests

    [Fact]
    public async Task UpdateItemQuantityAsync_ShouldReturnFailure_WhenItemDoesNotExist()
    {
        var userId = Guid.NewGuid();

        var result = await _sut.UpdateItemQuantityAsync(userId, Guid.NewGuid(), new UpdateCartItemRequest(5));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Cart item not found");
    }

    #endregion

    #region RemoveItemAsync Tests

    [Fact]
    public async Task RemoveItemAsync_ShouldReturnFailure_WhenItemDoesNotExist()
    {
        var userId = Guid.NewGuid();

        var result = await _sut.RemoveItemAsync(userId, Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Cart item not found");
    }

    #endregion

    private static Product CreateProduct(string name, decimal price, int stockQuantity = 100)
    {
        return Product.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            name,
            name.ToLower().Replace(" ", "-"),
            price,
            $"SKU-{Guid.NewGuid():N}",
            stockQuantity
        );
    }
}
