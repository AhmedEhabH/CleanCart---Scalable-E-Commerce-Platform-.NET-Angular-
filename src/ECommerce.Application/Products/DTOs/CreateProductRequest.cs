namespace ECommerce.Application.Products.DTOs;

public record CreateProductRequest(
    Guid CategoryId,
    string Name,
    string Slug,
    decimal Price,
    string SKU,
    int StockQuantity,
    string? Description = null,
    decimal? CompareAtPrice = null,
    int LowStockThreshold = 10,
    bool IsFeatured = false
);
