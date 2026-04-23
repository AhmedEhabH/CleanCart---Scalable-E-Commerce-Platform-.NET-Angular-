namespace ECommerce.Application.Products.DTOs;

/// <summary>
/// Product data transfer object with full product details
/// </summary>
/// <param name="Id">Unique product identifier</param>
/// <param name="VendorId">Vendor who owns this product</param>
/// <param name="CategoryId">Category the product belongs to</param>
/// <param name="Name">Product name</param>
/// <param name="Slug">URL-friendly slug</param>
/// <param name="Description">Product description</param>
/// <param name="Price">Current product price</param>
/// <param name="CompareAtPrice">Original price for discount comparison</param>
/// <param name="SKU">Stock keeping unit</param>
/// <param name="StockQuantity">Available stock quantity</param>
/// <param name="LowStockThreshold">Low stock alert threshold</param>
/// <param name="IsFeatured">Whether the product is featured</param>
/// <param name="IsActive">Whether the product is active</param>
/// <param name="ReviewCount">Total number of reviews</param>
/// <param name="AverageRating">Average rating from reviews</param>
/// <param name="IsInStock">Whether the product is in stock</param>
/// <param name="IsLowStock">Whether the product is low on stock</param>
/// <param name="HasDiscount">Whether the product has a discount</param>
/// <param name="DiscountPercentage">Discount percentage if applicable</param>
/// <param name="MainImageUrl">Main product image URL</param>
/// <param name="Images">Collection of product images</param>
/// <param name="CreatedAt">Creation timestamp</param>
/// <param name="UpdatedAt">Last update timestamp</param>
public record ProductDto(
    Guid Id,
    Guid? VendorId,
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
