using ECommerce.Application.Common.Models;
using ECommerce.Application.Products.DTOs;
using ECommerce.Application.Products.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;

namespace ECommerce.Application.Products.Services;

public class ResilientProductService : IProductService
{
    private readonly IProductService _inner;
    private readonly ILogger<ResilientProductService> _logger;
    private readonly ResiliencePipeline _pipeline;

    public ResilientProductService(IProductService inner, ILogger<ResilientProductService> logger)
    {
        _inner = inner;
        _logger = logger;
        _pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new Polly.Retry.RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    _logger.LogWarning(args.Outcome.Exception,
                        "Retry {Attempt}/{MaxRetries} after {Delay}ms",
                        args.AttemptNumber + 1, 3, args.RetryDelay.TotalMilliseconds);
                    return default;
                }
            })
            .Build();
    }

    public async Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _pipeline.ExecuteAsync(
            (CancellationToken ct) => new ValueTask<Result<ProductDto>>(_inner.GetByIdAsync(id, ct)),
            cancellationToken);
    }

    public async Task<Result<PaginatedResult<ProductDto>>> GetPagedAsync(ProductListQuery query, CancellationToken cancellationToken = default)
    {
        return await _pipeline.ExecuteAsync(
            (CancellationToken ct) => new ValueTask<Result<PaginatedResult<ProductDto>>>(_inner.GetPagedAsync(query, ct)),
            cancellationToken);
    }

    public async Task<Result<ProductDetailResponse>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _pipeline.ExecuteAsync(
            (CancellationToken ct) => new ValueTask<Result<ProductDetailResponse>>(_inner.GetBySlugAsync(slug, ct)),
            cancellationToken);
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> GetFeaturedAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _inner.GetFeaturedAsync(count, cancellationToken);
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _inner.GetByCategoryAsync(categoryId, cancellationToken);
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _inner.SearchAsync(searchTerm, cancellationToken);
    }

    public async Task<Result<ProductDto>> CreateAsync(Guid? vendorId, CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        return await _inner.CreateAsync(vendorId, request, cancellationToken);
    }

    public async Task<Result<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request, Guid? currentUserId, bool isAdmin, bool isSeller, CancellationToken cancellationToken = default)
    {
        return await _inner.UpdateAsync(id, request, currentUserId, isAdmin, isSeller, cancellationToken);
    }

    public async Task<Result> DeleteAsync(Guid id, Guid? currentUserId, bool isAdmin, bool isSeller, CancellationToken cancellationToken = default)
    {
        return await _inner.DeleteAsync(id, currentUserId, isAdmin, isSeller, cancellationToken);
    }

    public async Task<Result> ToggleActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _inner.ToggleActiveAsync(id, cancellationToken);
    }

    public async Task<Result> ToggleFeaturedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _inner.ToggleFeaturedAsync(id, cancellationToken);
    }

    public async Task<Result<ProductDto>> AddImageAsync(Guid productId, string imageUrl, string? altText, int displayOrder, Guid? currentUserId, bool isAdmin, bool isSeller, CancellationToken cancellationToken = default)
    {
        return await _inner.AddImageAsync(productId, imageUrl, altText, displayOrder, currentUserId, isAdmin, isSeller, cancellationToken);
    }

    public async Task<Result> RemoveImageAsync(Guid productId, Guid imageId, CancellationToken cancellationToken = default)
    {
        return await _inner.RemoveImageAsync(productId, imageId, cancellationToken);
    }
}
