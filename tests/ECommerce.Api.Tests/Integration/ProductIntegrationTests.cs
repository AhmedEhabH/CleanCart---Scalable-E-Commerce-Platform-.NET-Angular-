using ECommerce.Application.Products.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using FluentAssertions;

namespace ECommerce.Api.Tests.Integration;

public class ProductIntegrationTests
{
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
    public async Task CreateProduct_ShouldPersistToDatabase()
    {
        using var context = CreateContext();
        
        var category = Category.Create("Electronics", "electronics", "Electronic products");
        TestDatabaseFixture.SetPrivateProperty(category, "Id", TestCategoryId);
        context.Categories.Add(category);
        
        await context.SaveChangesAsync();
        
        context.Products.Should().BeEmpty();
        
        var product = Product.Create(
            null,
            TestCategoryId,
            "Test Product",
            "test-product",
            99.99m,
            "TEST001",
            100
        );
        
        context.Products.Add(product);
        await context.SaveChangesAsync();
        
        context.Products.Should().HaveCount(1);
        var saved = await context.Products.FirstAsync();
        saved.Name.Should().Be("Test Product");
        saved.SKU.Should().Be("TEST001");
        saved.VendorId.Should().BeNull();
    }

    [Fact]
    public async Task UpdateProduct_ShouldPersistChanges()
    {
        using var context = CreateContext();
        
        var product = Product.Create(
            TestVendorId,
            TestCategoryId,
            "Original",
            "original",
            10.00m,
            "SKU001",
            50
        );
        context.Products.Add(product);
        await context.SaveChangesAsync();
        
        var existing = await context.Products.FirstAsync();
        existing.Update("Updated Name", null, 20.00m, null, null, null);
        
        await context.SaveChangesAsync();
        
        var updated = await context.Products.FirstAsync();
        updated.Name.Should().Be("Updated Name");
        updated.Price.Should().Be(20.00m);
    }

    [Fact]
    public async Task DeleteProduct_ShouldRemoveFromDatabase()
    {
        using var context = CreateContext();
        
        var product = Product.Create(
            TestVendorId,
            TestCategoryId,
            "To Delete",
            "to-delete",
            10.00m,
            "DEL001",
            10
        );
        context.Products.Add(product);
        await context.SaveChangesAsync();
        
        context.Products.Should().HaveCount(1);
        
        context.Products.Remove(product);
        await context.SaveChangesAsync();
        
        context.Products.Should().BeEmpty();
    }

    [Fact]
    public async Task Product_WithNullVendorId_ShouldBeRetrievable()
    {
        using var context = CreateContext();
        
        var category = Category.Create("Test Category", "test-cat");
        TestDatabaseFixture.SetPrivateProperty(category, "Id", TestCategoryId);
        context.Categories.Add(category);
        
        var product = Product.Create(
            null,
            TestCategoryId,
            "Admin Product",
            "admin-product",
            50.00m,
            "ADM001",
            25
        );
        context.Products.Add(product);
        await context.SaveChangesAsync();
        
        var retrieved = await context.Products.FirstOrDefaultAsync(p => p.SKU == "ADM001");
        
        retrieved.Should().NotBeNull();
        retrieved!.VendorId.Should().BeNull();
        retrieved.Name.Should().Be("Admin Product");
    }
}