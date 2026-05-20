namespace ECommerce.Application.Wishlist.DTOs;

public record WishlistItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal Price,
    string? MainImageUrl,
    DateTime AddedAt
);