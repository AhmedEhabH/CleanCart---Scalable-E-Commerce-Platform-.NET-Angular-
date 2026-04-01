using ECommerce.Application.Cart.Interfaces;
using ECommerce.Application.Orders.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Xunit;
using CartEntity = ECommerce.Domain.Entities.Cart;
using ProductEntity = ECommerce.Domain.Entities.Product;

namespace ECommerce.Application.Tests.Services;

public class OrderServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICartService> _mockCartService;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new ApplicationDbContext(options);
        _mockCartService = new Mock<ICartService>();
        _sut = new OrderService(_context, _mockCartService.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region CreateOrderAsync Tests

    [Fact]
    public async Task CreateOrderAsync_ShouldReturnFailure_WhenCartIsEmpty()
    {
        var userId = Guid.NewGuid();

        var result = await _sut.CreateOrderAsync(userId, CreateOrderRequest());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Cart is empty");
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldCreateOrder_WhenCartHasItems()
    {
        var userId = Guid.NewGuid();
        var product = CreateProduct("Test Product", 50.00m, stockQuantity: 10);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var cart = CartEntity.CreateForUser(userId);
        var cartItem = cart.AddItem(product.Id, 2);
        cartItem.SetUnitPrice(product.Price);
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        var result = await _sut.CreateOrderAsync(userId, CreateOrderRequest());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.OrderNumber.Should().StartWith("ORD-");
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().ProductName.Should().Be("Test Product");
        result.Value.Items.First().Quantity.Should().Be(2);
        result.Value.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldCalculateTaxAndShipping()
    {
        var userId = Guid.NewGuid();
        var product = CreateProduct("Test Product", 50.00m, stockQuantity: 10);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var cart = CartEntity.CreateForUser(userId);
        var cartItem = cart.AddItem(product.Id, 1);
        cartItem.SetUnitPrice(product.Price);
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        var result = await _sut.CreateOrderAsync(userId, CreateOrderRequest());

        result.IsSuccess.Should().BeTrue();
        result.Value!.SubTotal.Should().Be(50.00m);
        result.Value.TaxAmount.Should().Be(5.00m);
        result.Value.ShippingCost.Should().Be(10.00m);
        result.Value.TotalAmount.Should().Be(65.00m);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldProvideFreeShipping_WhenSubtotalExceeds100()
    {
        var userId = Guid.NewGuid();
        var product = CreateProduct("Expensive Product", 120.00m, stockQuantity: 10);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var cart = CartEntity.CreateForUser(userId);
        var cartItem = cart.AddItem(product.Id, 1);
        cartItem.SetUnitPrice(product.Price);
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        var result = await _sut.CreateOrderAsync(userId, CreateOrderRequest());

        result.IsSuccess.Should().BeTrue();
        result.Value!.SubTotal.Should().Be(120.00m);
        result.Value.ShippingCost.Should().Be(0m);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldReturnFailure_WhenInsufficientStock()
    {
        var userId = Guid.NewGuid();
        var product = CreateProduct("Low Stock Product", 50.00m, stockQuantity: 1);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var cart = CartEntity.CreateForUser(userId);
        var cartItem = cart.AddItem(product.Id, 5);
        cartItem.SetUnitPrice(product.Price);
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        var result = await _sut.CreateOrderAsync(userId, CreateOrderRequest());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Insufficient stock");
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldReturnFailure_WhenProductIsInactive()
    {
        var userId = Guid.NewGuid();
        var product = CreateProduct("Inactive Product", 50.00m, stockQuantity: 10);
        product.Deactivate();
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var cart = CartEntity.CreateForUser(userId);
        var cartItem = cart.AddItem(product.Id, 1);
        cartItem.SetUnitPrice(product.Price);
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        var result = await _sut.CreateOrderAsync(userId, CreateOrderRequest());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found or inactive");
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldDeductStock_WhenOrderIsCreated()
    {
        var userId = Guid.NewGuid();
        var product = CreateProduct("Test Product", 50.00m, stockQuantity: 10);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var cart = CartEntity.CreateForUser(userId);
        var cartItem = cart.AddItem(product.Id, 3);
        cartItem.SetUnitPrice(product.Price);
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        await _sut.CreateOrderAsync(userId, CreateOrderRequest());

        var updatedProduct = await _context.Products.FindAsync(product.Id);
        updatedProduct!.StockQuantity.Should().Be(7);
    }

    #endregion

    #region GetOrderByIdAsync Tests

    [Fact]
    public async Task GetOrderByIdAsync_ShouldReturnFailure_WhenOrderDoesNotExist()
    {
        var userId = Guid.NewGuid();

        var result = await _sut.GetOrderByIdAsync(Guid.NewGuid(), userId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Order not found");
    }

    [Fact]
    public async Task GetOrderByIdAsync_ShouldReturnFailure_WhenOrderBelongsToDifferentUser()
    {
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var product = CreateProduct("Test Product", 50.00m, stockQuantity: 10);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var cart = CartEntity.CreateForUser(userId);
        var cartItem = cart.AddItem(product.Id, 1);
        cartItem.SetUnitPrice(product.Price);
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        var createResult = await _sut.CreateOrderAsync(userId, CreateOrderRequest());
        var orderId = createResult.Value!.Id;

        var result = await _sut.GetOrderByIdAsync(orderId, differentUserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Order not found");
    }

    #endregion

    #region GetUserOrdersAsync Tests

    [Fact]
    public async Task GetUserOrdersAsync_ShouldReturnEmpty_WhenNoOrdersExist()
    {
        var userId = Guid.NewGuid();

        var result = await _sut.GetUserOrdersAsync(userId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    #endregion

    private static CreateOrderRequest CreateOrderRequest()
    {
        return new CreateOrderRequest(
            new AddressDto("123 Main St", "New York", "NY", "10001", "USA"),
            null,
            "Test order"
        );
    }

    private static ProductEntity CreateProduct(string name, decimal price, int stockQuantity = 100)
    {
        return ProductEntity.Create(
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
