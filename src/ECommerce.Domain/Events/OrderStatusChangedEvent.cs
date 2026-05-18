namespace ECommerce.Domain.Events;

public sealed record OrderStatusChangedEvent(
    Guid OrderId,
    string OldStatus,
    string NewStatus,
    string CustomerEmail);
