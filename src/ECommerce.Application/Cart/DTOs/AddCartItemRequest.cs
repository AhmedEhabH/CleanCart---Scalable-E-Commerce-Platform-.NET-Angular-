namespace ECommerce.Application.Cart.DTOs;

public record AddCartItemRequest(Guid ProductId, int Quantity);
