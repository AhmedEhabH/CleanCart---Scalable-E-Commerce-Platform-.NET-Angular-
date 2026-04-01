namespace ECommerce.Application.Products.DTOs;

public record ProductDetailResponse(
    ProductDto Product,
    string? CategoryName,
    string? CategorySlug
);
