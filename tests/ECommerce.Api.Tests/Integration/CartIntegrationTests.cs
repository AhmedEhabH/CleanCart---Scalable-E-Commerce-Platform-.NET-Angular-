using ECommerce.Application.Cart.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using FluentAssertions;

namespace ECommerce.Api.Tests.Integration;

public class CartIntegrationTests
{
    private static readonly Guid TestUserId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid TestProductId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid TestVendorId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid TestCategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetCart_ShouldReturnEmptyCart_WhenUserHasNoCart()
    {
        using var context = CreateContext();
        var cartService = new CartService(context);

        var result = await cartService.GetCartAsync(TestUserId);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task GetCart_ShouldReturnCart_WhenUserHasItems()
    {
        using var context = CreateContext();
        
        var category = Category.Create("Electronics", "electronics", "Test");
        TestDatabaseFixture.SetPrivateProperty(category, "Id", TestCategoryId);
        context.Categories.Add(category);
        
        var product = Product.Create(TestVendorId, TestCategoryId, "Test Product", "test-product", 99.99m, "TEST001", 100);
        TestDatabaseFixture.SetPrivateProperty(product, "Id", TestProductId);
        context.Products.Add(product);
        
        var cart = Cart.CreateForUser(TestUserId);
        var item = cart.AddItem(TestProductId, 2);
        item.SetUnitPrice(99.99m);
        context.Carts.Add(cart);
        
        await context.SaveChangesAsync();
        
        var cartService = new CartService(context);
        var result = await cartService.GetCartAsync(TestUserId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddItem_ShouldFail_WhenProductNotFound()
    {
        using var context = CreateContext();
        var cartService = new CartService(context);

        var result = await cartService.AddItemAsync(TestUserId, new AddCartItemRequest(Guid.NewGuid(), 1));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Product not found");
    }

    [Fact]
    public async Task AddItem_ShouldFail_WhenInsufficientStock()
    {
        using var context = CreateContext();
        
        var category = Category.Create("Electronics", "electronics", "Test");
        TestDatabaseFixture.SetPrivateProperty(category, "Id", TestCategoryId);
        context.Categories.Add(category);
        
        var product = Product.Create(TestVendorId, TestCategoryId, "Test Product", "test-product", 99.99m, "TEST001", 10);
        TestDatabaseFixture.SetPrivateProperty(product, "Id", TestProductId);
        context.Products.Add(product);
        await context.SaveChangesAsync();
        
        var cartService = new CartService(context);

        var result = await cartService.AddItemAsync(TestUserId, new AddCartItemRequest(TestProductId, 100));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Insufficient stock");
    }
}