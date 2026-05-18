namespace ECommerce.Application.Common.Interfaces;

public interface ICartCleanupService
{
    Task CleanupAbandonedCartsAsync(CancellationToken cancellationToken = default);
}
