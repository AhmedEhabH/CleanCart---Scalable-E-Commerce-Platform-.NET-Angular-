namespace ECommerce.Application.Products.DTOs;

/// <summary>
/// Request to update an existing product
/// </summary>
/// <param name="Name">Optional product name</param>
/// <param name="Description">Optional product description</param>
/// <param name="Price">Optional product price</param>
/// <param name="CompareAtPrice">Optional original price for showing discounts</param>
/// <param name="LowStockThreshold">Optional threshold for low stock alerts</param>
/// <param name="IsFeatured">Optional whether the product is featured</param>
/// <param name="Image">Optional product image file</param>
public record UpdateProductRequest(
    string? Name = null,
    string? Description = null,
    decimal? Price = null,
    decimal? CompareAtPrice = null,
    int? LowStockThreshold = null,
    bool? IsFeatured = null,
    string? ImageUrl = null
);
