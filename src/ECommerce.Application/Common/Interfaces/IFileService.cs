namespace ECommerce.Application.Common.Interfaces;

public interface IFileService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string subfolder, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
}
