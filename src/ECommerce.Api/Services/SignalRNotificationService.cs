using ECommerce.Application.Common.Interfaces;
using ECommerce.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.Api.Services;

public sealed class SignalRNotificationService(IHubContext<NotificationHub> hubContext)
    : INotificationService
{
    public async Task SendUserNotificationAsync(string userEmail, string message)
    {
        await hubContext.Clients.User(userEmail).SendAsync("ReceiveNotification", message);
    }

    public async Task BroadcastOrderStatusAsync(Guid orderId, string newStatus)
    {
        await hubContext.Clients.All.SendAsync("OrderStatusUpdated", orderId, newStatus);
    }
}
