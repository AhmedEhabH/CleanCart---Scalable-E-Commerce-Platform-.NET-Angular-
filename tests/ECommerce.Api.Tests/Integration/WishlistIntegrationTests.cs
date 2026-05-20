using ECommerce.Application.Wishlist.Services;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace ECommerce.Api.Tests.Integration;

public class WishlistIntegrationTests
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

    private static WishlistService CreateService(ApplicationDbContext context)
    {
        var logger = Mock.Of<ILogger<WishlistService>>();
        return new WishlistService(context, logger);
    }

    [Fact]
    public async Task ToggleWishlistItem_ShouldAdd_WhenAuthenticatedAndProductExists()
    {
        using var context = CreateContext();

        var category = Category.Create("Electronics", "electronics", "Test");
        TestDatabaseFixture.SetPrivateProperty(category, "Id", TestCategoryId);
        context.Categories.Add(category);

        var product = Product.Create(TestVendorId, TestCategoryId, "Test Product", "test-product", 99.99m, "TEST001", 100);
        TestDatabaseFixture.SetPrivateProperty(product, "Id", TestProductId);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.ToggleWishlistItemAsync(TestUserId, TestProductId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        var storedWishlist = await context.Wishlists
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.UserId == TestUserId);
        storedWishlist.Should().NotBeNull();
        storedWishlist!.Items.Should().ContainSingle(i => i.ProductId == TestProductId);
    }

    [Fact]
    public async Task ToggleWishlistItem_ShouldRemove_WhenItemAlreadyInWishlist()
    {
        using var context = CreateContext();

        var category = Category.Create("Electronics", "electronics", "Test");
        TestDatabaseFixture.SetPrivateProperty(category, "Id", TestCategoryId);
        context.Categories.Add(category);

        var product = Product.Create(TestVendorId, TestCategoryId, "Test Product", "test-product", 99.99m, "TEST001", 100);
        TestDatabaseFixture.SetPrivateProperty(product, "Id", TestProductId);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        await service.ToggleWishlistItemAsync(TestUserId, TestProductId);

        var result = await service.ToggleWishlistItemAsync(TestUserId, TestProductId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();

        var storedWishlist = await context.Wishlists
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.UserId == TestUserId);
        storedWishlist!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ToggleWishlistItem_ShouldReturnFailure_WhenProductDoesNotExist()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var result = await service.ToggleWishlistItemAsync(TestUserId, Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Product not found");
    }

    [Fact]
    public async Task ToggleWishlistItem_ShouldCreateWishlist_WhenUserHasNoWishlist()
    {
        using var context = CreateContext();

        var category = Category.Create("Electronics", "electronics", "Test");
        TestDatabaseFixture.SetPrivateProperty(category, "Id", TestCategoryId);
        context.Categories.Add(category);

        var product = Product.Create(TestVendorId, TestCategoryId, "Test Product", "test-product", 99.99m, "TEST001", 100);
        TestDatabaseFixture.SetPrivateProperty(product, "Id", TestProductId);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var wishlistBefore = await context.Wishlists.FirstOrDefaultAsync(w => w.UserId == TestUserId);
        wishlistBefore.Should().BeNull();

        var result = await service.ToggleWishlistItemAsync(TestUserId, TestProductId);

        result.IsSuccess.Should().BeTrue();

        var wishlistAfter = await context.Wishlists.FirstOrDefaultAsync(w => w.UserId == TestUserId);
        wishlistAfter.Should().NotBeNull();
    }
}
