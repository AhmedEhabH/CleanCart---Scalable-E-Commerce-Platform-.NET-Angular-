namespace ECommerce.Application.Products.DTOs;

public record UpdateProductRequest(
    string? Name = null,
    string? Description = null,
    decimal? Price = null,
    decimal? CompareAtPrice = null,
    int? LowStockThreshold = null,
    bool? IsFeatured = null
);
