namespace ECommerce.Application.Cart.DTOs;

public record CartDto(
    Guid Id,
    IReadOnlyList<CartItemDto> Items,
    int TotalItems,
    decimal SubTotal,
    bool IsEmpty
);
