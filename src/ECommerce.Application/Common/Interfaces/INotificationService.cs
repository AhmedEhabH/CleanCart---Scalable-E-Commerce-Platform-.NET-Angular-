namespace ECommerce.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendUserNotificationAsync(string userEmail, string message);
    Task BroadcastOrderStatusAsync(Guid orderId, string newStatus);
}
