namespace ECommerce.Application.Common.Interfaces;

public interface IAiService
{
    Task<string> GetChatResponseAsync(string userMessage, CancellationToken cancellationToken = default);
    Task<string> GetChatResponseWithHistoryAsync(List<ChatMessage> messages, CancellationToken cancellationToken = default);
}

public record ChatMessage(string Role, string Content);
