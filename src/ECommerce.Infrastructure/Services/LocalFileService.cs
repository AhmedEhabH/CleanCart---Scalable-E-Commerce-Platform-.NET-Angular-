using ECommerce.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class LocalFileService : IFileService
{
    private readonly string _rootPath;
    private readonly ILogger<LocalFileService> _logger;

    public LocalFileService(IWebHostEnvironment environment, ILogger<LocalFileService> logger)
    {
        var basePath = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        _rootPath = Path.Combine(basePath, "images");
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string subfolder, CancellationToken cancellationToken = default)
    {
        var dir = Path.Combine(_rootPath, subfolder);

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
            _logger.LogInformation("Created directory: {Directory}", dir);
        }

        var ext = Path.GetExtension(fileName);
        var uniqueName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(dir, uniqueName);

        try
        {
            await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous);
            await fileStream.CopyToAsync(fs, cancellationToken);
            _logger.LogInformation("File saved successfully: {FilePath}", filePath);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "IO error saving file: {Message}", ex.Message);
            throw;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Unauthorized access error saving file: {Message}", ex.Message);
            throw;
        }

        return $"/images/{subfolder}/{uniqueName}";
    }

    public Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var relativePath = filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_rootPath, relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}
