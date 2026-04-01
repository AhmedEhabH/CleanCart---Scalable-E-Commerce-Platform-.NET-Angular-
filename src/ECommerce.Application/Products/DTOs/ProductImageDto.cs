namespace ECommerce.Application.Products.DTOs;

public record ProductImageDto(
    Guid Id,
    string ImageUrl,
    string? AltText,
    int DisplayOrder
);
