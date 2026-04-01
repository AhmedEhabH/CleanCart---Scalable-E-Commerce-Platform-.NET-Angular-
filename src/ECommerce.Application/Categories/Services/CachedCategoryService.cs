using ECommerce.Application.Categories.DTOs;
using ECommerce.Application.Categories.Interfaces;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;

namespace ECommerce.Application.Categories.Services;

public class CachedCategoryService : ICategoryService
{
    private readonly ICategoryService _inner;
    private readonly ICacheService _cache;

    public CachedCategoryService(ICategoryService inner, ICacheService cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"category:{id}";
        var cached = await _cache.GetAsync<Result<CategoryDto>>(cacheKey, cancellationToken);
        if (cached != null)
            return cached;

        var result = await _inner.GetByIdAsync(id, cancellationToken);
        if (result.IsSuccess)
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30), cancellationToken);

        return result;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "category:all";
        var cached = await _cache.GetAsync<Result<IReadOnlyList<CategoryDto>>>(cacheKey, cancellationToken);
        if (cached != null)
            return cached;

        var result = await _inner.GetAllAsync(cancellationToken);
        if (result.IsSuccess)
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30), cancellationToken);

        return result;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "category:root";
        var cached = await _cache.GetAsync<Result<IReadOnlyList<CategoryDto>>>(cacheKey, cancellationToken);
        if (cached != null)
            return cached;

        var result = await _inner.GetRootCategoriesAsync(cancellationToken);
        if (result.IsSuccess)
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30), cancellationToken);

        return result;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> GetSubcategoriesAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"category:subcategories:{parentId}";
        var cached = await _cache.GetAsync<Result<IReadOnlyList<CategoryDto>>>(cacheKey, cancellationToken);
        if (cached != null)
            return cached;

        var result = await _inner.GetSubcategoriesAsync(parentId, cancellationToken);
        if (result.IsSuccess)
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30), cancellationToken);

        return result;
    }

    public async Task<Result<CategoryDto>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"category:slug:{slug}";
        var cached = await _cache.GetAsync<Result<CategoryDto>>(cacheKey, cancellationToken);
        if (cached != null)
            return cached;

        var result = await _inner.GetBySlugAsync(slug, cancellationToken);
        if (result.IsSuccess)
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30), cancellationToken);

        return result;
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _inner.CreateAsync(request, cancellationToken);
        if (result.IsSuccess)
            await _cache.RemoveByPrefixAsync("category:", cancellationToken);
        return result;
    }

    public async Task<Result<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _inner.UpdateAsync(id, request, cancellationToken);
        if (result.IsSuccess)
            await _cache.RemoveByPrefixAsync("category:", cancellationToken);
        return result;
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _inner.DeleteAsync(id, cancellationToken);
        if (result.IsSuccess)
            await _cache.RemoveByPrefixAsync("category:", cancellationToken);
        return result;
    }
}
