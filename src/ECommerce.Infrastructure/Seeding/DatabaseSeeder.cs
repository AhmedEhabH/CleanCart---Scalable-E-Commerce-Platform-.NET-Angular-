using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Seeding;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await SeedAdminUserAsync(context);
        await SeedCategoriesAsync(context);
        await SeedProductsAsync(context);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAdminUserAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync(u => u.Email == "admin@ecommerce.com"))
            return;

        var admin = User.Create(
            email: "admin@ecommerce.com",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("Admin@123", 12),
            firstName: "System",
            lastName: "Administrator",
            role: Role.Admin
        );
        admin.ConfirmEmail();
        admin.Activate();

        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync())
            return;

        var electronics = Category.Create("Electronics", "electronics", "Electronic devices and accessories", displayOrder: 1);
        var clothing = Category.Create("Clothing", "clothing", "Apparel and fashion items", displayOrder: 2);
        var home = Category.Create("Home & Garden", "home-garden", "Home improvement and garden supplies", displayOrder: 3);

        context.Categories.AddRange(electronics, clothing, home);
        await context.SaveChangesAsync();

        var phones = Category.Create("Phones", "phones", "Smartphones and accessories", parentId: electronics.Id, displayOrder: 1);
        var laptops = Category.Create("Laptops", "laptops", "Laptops and notebooks", parentId: electronics.Id, displayOrder: 2);
        var mensClothing = Category.Create("Men's Clothing", "mens-clothing", "Clothing for men", parentId: clothing.Id, displayOrder: 1);
        var womensClothing = Category.Create("Women's Clothing", "womens-clothing", "Clothing for women", parentId: clothing.Id, displayOrder: 2);

        context.Categories.AddRange(phones, laptops, mensClothing, womensClothing);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(ApplicationDbContext context)
    {
        if (await context.Products.AnyAsync())
            return;

        var admin = await context.Users.FirstAsync(u => u.Email == "admin@ecommerce.com");
        var electronics = await context.Categories.FirstAsync(c => c.Slug == "electronics");
        var phones = await context.Categories.FirstAsync(c => c.Slug == "phones");
        var laptops = await context.Categories.FirstAsync(c => c.Slug == "laptops");
        var clothing = await context.Categories.FirstAsync(c => c.Slug == "clothing");
        var home = await context.Categories.FirstAsync(c => c.Slug == "home-garden");

        var vendor = Vendor.Create(admin.Id, "Demo Store", "Official demo vendor store", contactEmail: "store@demo.com");
        vendor.Approve();
        context.Vendors.Add(vendor);
        await context.SaveChangesAsync();

        var products = new[]
        {
            Product.Create(vendor.Id, phones.Id, "Premium Smartphone", "premium-smartphone", 999.99m, "PHONE-001", 150, "Latest flagship smartphone with advanced camera and long battery life", 1199.99m, isFeatured: true),
            Product.Create(vendor.Id, phones.Id, "Budget Smartphone", "budget-smartphone", 299.99m, "PHONE-002", 300, "Affordable smartphone with great value for everyday use"),
            Product.Create(vendor.Id, laptops.Id, "Gaming Laptop", "gaming-laptop", 1499.99m, "LAPTOP-001", 50, "High-performance gaming laptop with dedicated GPU", 1799.99m, isFeatured: true),
            Product.Create(vendor.Id, laptops.Id, "Ultrabook", "ultrabook", 899.99m, "LAPTOP-002", 75, "Thin and light laptop for professionals on the go"),
            Product.Create(vendor.Id, electronics.Id, "Wireless Earbuds", "wireless-earbuds", 79.99m, "AUDIO-001", 500, "Premium wireless earbuds with noise cancellation", 99.99m),
            Product.Create(vendor.Id, clothing.Id, "Classic T-Shirt", "classic-tshirt", 24.99m, "SHIRT-001", 1000, "Comfortable cotton t-shirt in various colors"),
            Product.Create(vendor.Id, home.Id, "Smart LED Bulb", "smart-led-bulb", 14.99m, "HOME-001", 800, "WiFi-enabled smart LED bulb with color changing", 19.99m),
            Product.Create(vendor.Id, home.Id, "Garden Tool Set", "garden-tool-set", 49.99m, "HOME-002", 200, "Complete 10-piece garden tool set with carrying case"),
        };

        context.Products.AddRange(products);

        foreach (var product in products)
        {
            product.AddImage($"https://via.placeholder.com/400x400?text={Uri.EscapeDataString(product.Name)}", product.Name, 0);
        }

        await context.SaveChangesAsync();
    }
}
