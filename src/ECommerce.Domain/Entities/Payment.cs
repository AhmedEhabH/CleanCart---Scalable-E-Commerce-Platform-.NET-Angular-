using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Order? Order { get; private set; }
    public PaymentMethod Method { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string? TransactionId { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public string? ProviderResponse { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private Payment() { }

    public static Payment Create(
        Guid orderId,
        PaymentMethod method,
        string provider,
        decimal amount,
        string currency = "USD")
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));

        return new Payment
        {
            OrderId = orderId,
            Method = method,
            Provider = provider.Trim(),
            Amount = Math.Round(amount, 2),
            Currency = currency.Trim().ToUpperInvariant(),
            Status = PaymentStatus.Pending
        };
    }

    public void Authorize(string transactionId, string? providerResponse = null)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be authorized");

        TransactionId = transactionId;
        ProviderResponse = providerResponse;
        Status = PaymentStatus.Authorized;
        MarkAsUpdated();
    }

    public void MarkAsPaid(string? providerResponse = null)
    {
        if (Status != PaymentStatus.Authorized && Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only authorized or pending payments can be marked as paid");

        TransactionId ??= $"TXN-{Guid.NewGuid():N}";
        ProviderResponse = providerResponse;
        Status = PaymentStatus.Paid;
        ProcessedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void MarkAsFailed(string reason, string? providerResponse = null)
    {
        ProviderResponse = providerResponse;
        FailureReason = reason;
        Status = PaymentStatus.Failed;
        MarkAsUpdated();
    }

    public void MarkAsRefunded(decimal? refundAmount = null, string? providerResponse = null)
    {
        if (Status != PaymentStatus.Paid)
            throw new InvalidOperationException("Only paid payments can be refunded");

        if (refundAmount.HasValue && refundAmount.Value < Amount)
        {
            Status = PaymentStatus.PartiallyRefunded;
        }
        else
        {
            Status = PaymentStatus.Refunded;
        }

        ProviderResponse = providerResponse;
        ProcessedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public bool IsSuccessful => Status == PaymentStatus.Paid;
    public bool IsPending => Status == PaymentStatus.Pending;
    public bool IsFailed => Status == PaymentStatus.Failed;
    public bool IsRefunded => Status == PaymentStatus.Refunded || Status == PaymentStatus.PartiallyRefunded;
}
