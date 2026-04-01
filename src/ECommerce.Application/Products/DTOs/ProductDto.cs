namespace ECommerce.Application.Products.DTOs;

public record ProductDto(
    Guid Id,
    Guid VendorId,
    Guid CategoryId,
    string Name,
    string Slug,
    string? Description,
    decimal Price,
    decimal? CompareAtPrice,
    string SKU,
    int StockQuantity,
    int LowStockThreshold,
    bool IsFeatured,
    bool IsActive,
    int ReviewCount,
    decimal AverageRating,
    bool IsInStock,
    bool IsLowStock,
    bool HasDiscount,
    decimal DiscountPercentage,
    string? MainImageUrl,
    IReadOnlyList<ProductImageDto> Images,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
