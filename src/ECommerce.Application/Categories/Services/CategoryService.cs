using ECommerce.Application.Categories.DTOs;
using ECommerce.Application.Categories.Interfaces;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Categories.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILoggerService _logger;

    public CategoryService(ICategoryRepository categoryRepository, ILoggerService logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null)
        {
            _logger.LogWarning("Category not found: {CategoryId}", id);
            return Result<CategoryDto>.Failure("Category not found", "CATEGORY_NOT_FOUND");
        }

        var productCounts = await _categoryRepository.GetProductCountsAsync(cancellationToken);
        var children = await _categoryRepository.GetSubcategoriesAsync(id, cancellationToken);
        var count = productCounts.TryGetValue(category.Id, out var value) ? value : 0;

        var dto = new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ParentId,
            category.IconUrl,
            category.IsActive,
            category.DisplayOrder,
            children.Select(c => MapToSimpleDto(c, productCounts)).ToList(),
            count,
            category.CreatedAt,
            category.UpdatedAt ?? category.CreatedAt
        );

        return Result<CategoryDto>.Success(dto);
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        var productCounts = await _categoryRepository.GetProductCountsAsync(cancellationToken);

        var dtos = categories
            .Where(c => c.ParentId == null)
            .Select(c => MapToTreeDto(c, categories, productCounts))
            .ToList();

        _logger.LogDebug("Retrieved {Count} root categories", dtos.Count);

        return Result<IReadOnlyList<CategoryDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetRootCategoriesAsync(cancellationToken);
        var productCounts = await _categoryRepository.GetProductCountsAsync(cancellationToken);

        var dtos = categories
            .Select(c => MapToSimpleDto(c, productCounts))
            .ToList();

        return Result<IReadOnlyList<CategoryDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> GetSubcategoriesAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        var parentExists = await _categoryRepository.ExistsAsync(parentId, cancellationToken);
        if (!parentExists)
        {
            _logger.LogWarning("Parent category not found: {ParentId}", parentId);
            return Result<IReadOnlyList<CategoryDto>>.Failure("Parent category not found", "CATEGORY_NOT_FOUND");
        }

        var subcategories = await _categoryRepository.GetSubcategoriesAsync(parentId, cancellationToken);
        var productCounts = await _categoryRepository.GetProductCountsAsync(cancellationToken);

        var dtos = subcategories
            .Select(c => MapToSimpleDto(c, productCounts))
            .ToList();

        return Result<IReadOnlyList<CategoryDto>>.Success(dtos);
    }

    public async Task<Result<CategoryDto>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetBySlugAsync(slug, cancellationToken);
        if (category == null)
        {
            _logger.LogWarning("Category not found by slug: {Slug}", slug);
            return Result<CategoryDto>.Failure("Category not found", "CATEGORY_NOT_FOUND");
        }

        var productCounts = await _categoryRepository.GetProductCountsAsync(cancellationToken);
        var count = productCounts.TryGetValue(category.Id, out var value) ? value : 0;

        var dto = new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ParentId,
            category.IconUrl,
            category.IsActive,
            category.DisplayOrder,
            Array.Empty<CategoryDto>(),
            count,
            category.CreatedAt,
            category.UpdatedAt ?? category.CreatedAt
        );

        return Result<CategoryDto>.Success(dto);
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var existingBySlug = await _categoryRepository.GetBySlugAsync(request.Slug, cancellationToken);
        if (existingBySlug != null)
        {
            _logger.LogWarning("Failed to create category: slug {Slug} already exists", request.Slug);
            return Result<CategoryDto>.Failure("A category with this slug already exists", "CATEGORY_SLUG_EXISTS");
        }

        if (request.ParentId.HasValue)
        {
            var parentExists = await _categoryRepository.ExistsAsync(request.ParentId.Value, cancellationToken);
            if (!parentExists)
            {
                _logger.LogWarning("Failed to create category: parent {ParentId} not found", request.ParentId.Value);
                return Result<CategoryDto>.Failure("Parent category not found", "CATEGORY_NOT_FOUND");
            }
        }

        var category = Category.Create(
            request.Name,
            request.Slug,
            request.Description,
            request.ParentId,
            request.IconUrl,
            request.DisplayOrder
        );

        await _categoryRepository.AddAsync(category, cancellationToken);

        _logger.LogInformation("Category created: {CategoryId}, Name: {Name}, ParentId: {ParentId}", category.Id, category.Name, request.ParentId?.ToString() ?? "none");

        return Result<CategoryDto>.Success(MapToSimpleDto(category, new Dictionary<Guid, int>()));
    }

    public async Task<Result<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null)
        {
            _logger.LogWarning("Failed to update category: {CategoryId} not found", id);
            return Result<CategoryDto>.Failure("Category not found", "CATEGORY_NOT_FOUND");
        }

        category.Update(
            request.Name ?? category.Name,
            request.Description,
            request.IconUrl,
            request.DisplayOrder ?? category.DisplayOrder
        );

        await _categoryRepository.UpdateAsync(category, cancellationToken);

        var productCounts = await _categoryRepository.GetProductCountsAsync(cancellationToken);
        var count = productCounts.TryGetValue(category.Id, out var value) ? value : 0;

        var dto = new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ParentId,
            category.IconUrl,
            category.IsActive,
            category.DisplayOrder,
            Array.Empty<CategoryDto>(),
            count,
            category.CreatedAt,
            category.UpdatedAt ?? category.CreatedAt
        );

        _logger.LogInformation("Category updated: {CategoryId}, Name: {Name}", category.Id, category.Name);

        return Result<CategoryDto>.Success(dto);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null)
        {
            _logger.LogWarning("Failed to delete category: {CategoryId} not found", id);
            return Result.Failure("Category not found", "CATEGORY_NOT_FOUND");
        }

        var hasSubcategories = await _categoryRepository.HasSubcategoriesAsync(id, cancellationToken);
        if (hasSubcategories)
        {
            _logger.LogWarning("Failed to delete category {CategoryId}: has subcategories", id);
            return Result.Failure("Cannot delete category with subcategories", "CATEGORY_HAS_SUBCATEGORIES");
        }

        var hasProducts = await _categoryRepository.HasProductsAsync(id, cancellationToken);
        if (hasProducts)
        {
            _logger.LogWarning("Failed to delete category {CategoryId}: has products", id);
            return Result.Failure("Cannot delete category with products", "CATEGORY_HAS_PRODUCTS");
        }

        await _categoryRepository.DeleteAsync(category, cancellationToken);

        _logger.LogInformation("Category deleted: {CategoryId}, Name: {Name}", category.Id, category.Name);

        return Result.Success();
    }

    private CategoryDto MapToSimpleDto(Category category, Dictionary<Guid, int> productCounts)
    {
        var count = productCounts.TryGetValue(category.Id, out var value) ? value : 0;

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ParentId,
            category.IconUrl,
            category.IsActive,
            category.DisplayOrder,
            Array.Empty<CategoryDto>(),
            count,
            category.CreatedAt,
            category.UpdatedAt ?? category.CreatedAt
        );
    }

    private CategoryDto MapToTreeDto(Category category, IReadOnlyList<Category> allCategories, Dictionary<Guid, int> productCounts)
    {
        var count = productCounts.TryGetValue(category.Id, out var value) ? value : 0;
        var children = allCategories.Where(c => c.ParentId == category.Id).ToList();

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ParentId,
            category.IconUrl,
            category.IsActive,
            category.DisplayOrder,
            children.Select(c => MapToTreeDto(c, allCategories, productCounts)).ToList(),
            count,
            category.CreatedAt,
            category.UpdatedAt ?? category.CreatedAt
        );
    }
}
