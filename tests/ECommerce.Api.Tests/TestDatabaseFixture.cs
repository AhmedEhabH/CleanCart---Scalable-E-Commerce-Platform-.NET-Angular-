using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Reflection;

namespace ECommerce.Api.Tests;

public class TestDatabaseFixture
{
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TestVendorId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid TestCategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid TestProductId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static ApplicationDbContext CreateInMemoryContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new ApplicationDbContext(options);
        return context;
    }

    public static async Task SeedTestDataAsync(ApplicationDbContext context)
    {
        var user = CreateUser(TestUserId, "test@example.com", "TestPassword123!", "Test", "User", "+1234567890");
        context.Users.Add(user);

        var category = CreateCategory(TestCategoryId, "Electronics", "electronics", "Electronic products");
        context.Categories.Add(category);

        await context.SaveChangesAsync();

        var product = Product.Create(TestVendorId, TestCategoryId, "Test Product", "test-product", 99.99m, "TEST001", 100);
        SetPrivateProperty(product, "Id", TestProductId);
        context.Products.Add(product);

        await context.SaveChangesAsync();
    }

    private static User CreateUser(Guid id, string email, string password, string firstName, string lastName, string? phone)
    {
        var user = User.Create(email, BCrypt.Net.BCrypt.HashPassword(password), firstName, lastName, phone);
        SetPrivateProperty(user, "Id", id);
        return user;
    }

    private static Category CreateCategory(Guid id, string name, string slug, string? description)
    {
        var category = Category.Create(name, slug, description);
        SetPrivateProperty(category, "Id", id);
        return category;
    }

    public static void SetPrivateProperty(object obj, string propertyName, object value)
    {
        var type = obj.GetType();
        while (type != null)
        {
            var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null)
            {
                var backingField = type.GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                if (backingField != null)
                {
                    backingField.SetValue(obj, value);
                    return;
                }
                var setter = property.GetSetMethod(true);
                if (setter != null)
                {
                    setter.Invoke(obj, new[] { value });
                    return;
                }
            }
            type = type.BaseType;
        }
    }
}