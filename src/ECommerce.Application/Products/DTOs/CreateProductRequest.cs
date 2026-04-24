namespace ECommerce.Application.Products.DTOs;

/// <summary>
/// Request to create a new product
/// </summary>
/// <param name="CategoryId">Category ID the product belongs to</param>
/// <param name="Name">Product name</param>
/// <param name="Slug">URL-friendly slug for the product</param>
/// <param name="Price">Product price</param>
/// <param name="SKU">Stock keeping unit identifier</param>
/// <param name="StockQuantity">Available stock quantity</param>
/// <param name="Description">Optional product description</param>
/// <param name="CompareAtPrice">Optional original price for showing discounts</param>
/// <param name="LowStockThreshold">Threshold for low stock alerts (default: 10)</param>
/// <param name="IsFeatured">Whether the product is featured (default: false)</param>
/// <param name="Image">Product image file</param>
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
    bool IsFeatured = false,
    IFormFile? Image = null
);
