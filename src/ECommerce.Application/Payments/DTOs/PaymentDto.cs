using ECommerce.Domain.Enums;

namespace ECommerce.Application.Payments.DTOs;

public record PaymentDto(
    Guid Id,
    Guid OrderId,
    string Method,
    string Provider,
    string? TransactionId,
    string Status,
    decimal Amount,
    string Currency,
    string? FailureReason,
    DateTime? ProcessedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreatePaymentRequest(
    Guid OrderId,
    PaymentMethod Method,
    string Provider,
    decimal Amount,
    string Currency = "USD"
);

public record ProcessPaymentRequest(
    Guid PaymentId,
    string? ProviderReference,
    string? ProviderResponse
);

public record RefundPaymentRequest(
    Guid PaymentId,
    decimal? RefundAmount,
    string? Reason
);
