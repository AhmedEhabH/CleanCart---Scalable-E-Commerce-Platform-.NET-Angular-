using ECommerce.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace ECommerce.Infrastructure.Services;

public class LocalFileService : IFileService
{
    private readonly string _rootPath;

    public LocalFileService(IWebHostEnvironment environment)
    {
        _rootPath = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "images");
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string subfolder, CancellationToken cancellationToken = default)
    {
        var dir = Path.Combine(_rootPath, subfolder);
        Directory.CreateDirectory(dir);

        var ext = Path.GetExtension(fileName);
        var uniqueName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(dir, uniqueName);

        await using var fs = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fs, cancellationToken);

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
