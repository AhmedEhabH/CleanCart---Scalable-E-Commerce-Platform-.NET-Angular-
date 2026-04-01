namespace ECommerce.Application.Common.Interfaces;

public interface ILoggerService
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(string message, string? errorCode = null, params object[] args);
    void LogDebug(string message, params object[] args);
}
