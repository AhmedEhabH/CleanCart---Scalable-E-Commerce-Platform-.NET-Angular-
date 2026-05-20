using ECommerce.Application.Common.Models;
using ECommerce.Application.Wishlist.DTOs;
using ECommerce.Application.Wishlist.Services;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace ECommerce.Application.Tests.Services;

public class WishlistServiceTests
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

    private static Product CreateTestProduct(ApplicationDbContext context)
    {
        var category = Category.Create("Electronics", "electronics", "Test");
        SetId(category, TestCategoryId);
        context.Categories.Add(category);

        var product = Product.Create(TestVendorId, TestCategoryId, "Test Product", "test-product", 99.99m, "TEST001", 100);
        SetId(product, TestProductId);
        context.Products.Add(product);

        context.SaveChanges();
        return product;
    }

    private static void SetId(object entity, Guid id)
    {
        var prop = entity.GetType().GetProperty("Id");
        if (prop?.SetMethod != null)
        {
            prop.SetValue(entity, id);
        }
        else
        {
            var field = entity.GetType().GetField("<Id>k__BackingField",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(entity, id);
        }
    }

    [Fact]
    public async Task ToggleWishlistItemAsync_ShouldReturnSuccess_WhenProductExists()
    {
        using var context = CreateContext();
        CreateTestProduct(context);
        var service = CreateService(context);

        var result = await service.ToggleWishlistItemAsync(TestUserId, TestProductId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        var toggleResult = await service.ToggleWishlistItemAsync(TestUserId, TestProductId);
        toggleResult.IsSuccess.Should().BeTrue();
        toggleResult.Value.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleWishlistItemAsync_ShouldReturnFailure_WhenProductDoesNotExist()
    {
        using var context = CreateContext();
        var service = CreateService(context);
        var missingProductId = Guid.NewGuid();

        var result = await service.ToggleWishlistItemAsync(TestUserId, missingProductId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Product not found");
    }

    [Fact]
    public async Task SyncWishlistAsync_ShouldMergeCorrectly()
    {
        using var context = CreateContext();
        CreateTestProduct(context);

        var secondProductId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        var secondProduct = Product.Create(TestVendorId, TestCategoryId, "Second Product", "second-product", 49.99m, "TEST002", 50);
        SetId(secondProduct, secondProductId);
        context.Products.Add(secondProduct);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var localIds = new List<Guid> { TestProductId, secondProductId };
        var result = await service.SyncWishlistAsync(TestUserId, localIds);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(i => i.ProductId).Should().Contain(new[] { TestProductId, secondProductId });
    }

    [Fact]
    public async Task SyncWishlistAsync_ShouldReturnEmpty_WhenNoLocalIds()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var result = await service.SyncWishlistAsync(TestUserId, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        var emptyResult = await service.SyncWishlistAsync(TestUserId, new List<Guid>());
        emptyResult.IsSuccess.Should().BeTrue();
        emptyResult.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task SyncWishlistAsync_ShouldNotDuplicate_WhenProductAlreadyInWishlist()
    {
        using var context = CreateContext();
        CreateTestProduct(context);
        var service = CreateService(context);

        await service.ToggleWishlistItemAsync(TestUserId, TestProductId);

        var localIds = new List<Guid> { TestProductId };
        var result = await service.SyncWishlistAsync(TestUserId, localIds);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(i => i.ProductId == TestProductId);
    }
}
