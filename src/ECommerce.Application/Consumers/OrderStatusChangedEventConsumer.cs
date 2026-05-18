using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Consumers;

public sealed class OrderStatusChangedEventConsumer(
    IEmailService emailService,
    INotificationService notificationService,
    ILogger<OrderStatusChangedEventConsumer> logger) : IConsumer<OrderStatusChangedEvent>
{
    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
    {
        var @event = context.Message;

        logger.LogInformation(
            "Order {OrderId} status changed from {OldStatus} to {NewStatus}",
            @event.OrderId, @event.OldStatus, @event.NewStatus);

        var subject = $"Order {@event.OrderId} status update";
        var body = $"Your order {@event.OrderId} has moved from {@event.OldStatus} to {@event.NewStatus}.";

        await emailService.SendEmailAsync(@event.CustomerEmail, subject, body, context.CancellationToken);

        await notificationService.SendUserNotificationAsync(
            @event.CustomerEmail,
            $"Your order {@event.OrderId} status is now {@event.NewStatus}!");

        await notificationService.BroadcastOrderStatusAsync(@event.OrderId, @event.NewStatus);
    }
}
