using ECommerce.Domain.ValueObjects;

namespace ECommerce.Application.Orders.DTOs;

public record CreateOrderRequest(
    AddressDto ShippingAddress,
    AddressDto? BillingAddress,
    string? Notes
);

public static class CreateOrderRequestExtensions
{
    public static Address ToAddress(this CreateOrderRequest request)
    {
        return Address.Create(
            request.ShippingAddress.Street,
            request.ShippingAddress.City,
            request.ShippingAddress.State,
            request.ShippingAddress.PostalCode,
            request.ShippingAddress.Country
        );
    }

    public static Address? ToBillingAddress(this CreateOrderRequest? request)
    {
        if (request?.BillingAddress == null) return null;
        return Address.Create(
            request.BillingAddress.Street,
            request.BillingAddress.City,
            request.BillingAddress.State,
            request.BillingAddress.PostalCode,
            request.BillingAddress.Country
        );
    }
}