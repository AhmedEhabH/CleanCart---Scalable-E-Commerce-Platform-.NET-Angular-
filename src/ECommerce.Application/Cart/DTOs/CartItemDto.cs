namespace ECommerce.Application.Cart.DTOs;

public record CartItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSlug,
    int Quantity,
    decimal UnitPrice,
    decimal Total,
    bool IsInStock
);
