using Microsoft.Extensions.Logging;
using ECommerce.Application.Common.Interfaces;

namespace ECommerce.Infrastructure.Services;

public class LoggerService : ILoggerService
{
    private readonly ILogger<LoggerService> _logger;

    public LoggerService(ILogger<LoggerService> logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message, params object[] args) => 
        _logger.LogInformation(message, args);

    public void LogWarning(string message, params object[] args) => 
        _logger.LogWarning(message, args);

    public void LogError(string message, string? errorCode = null, params object[] args) =>
        _logger.LogError(message, args);

    public void LogDebug(string message, params object[] args) => 
        _logger.LogDebug(message, args);
}
