using ECommerce.Application.Common.Models;
using ECommerce.Application.Products.DTOs;
using ECommerce.Application.Products.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Products.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetWithImagesAsync(id, cancellationToken);
        if (product == null)
            return Result<ProductDto>.Failure("Product not found", "PRODUCT_NOT_FOUND");

        return Result<ProductDto>.Success(MapToDto(product));
    }

    public async Task<Result<PaginatedResult<ProductDto>>> GetPagedAsync(ProductListQuery query, CancellationToken cancellationToken = default)
    {
        var queryable = _productRepository.GetQueryable();

        queryable = ApplyFilters(queryable, query);
        queryable = ApplySorting(queryable, query);

        var totalCount = await queryable.CountAsync(cancellationToken);

        var products = await queryable
            .Include(p => p.Images)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = products.Select(MapToDto).ToList();
        var result = new PaginatedResult<ProductDto>(dtos, totalCount, query.Page, query.PageSize);

        return Result<PaginatedResult<ProductDto>>.Success(result);
    }

    public async Task<Result<ProductDetailResponse>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetBySlugAsync(slug, cancellationToken);
        if (product == null)
            return Result<ProductDetailResponse>.Failure("Product not found", "PRODUCT_NOT_FOUND");

        var category = await _categoryRepository.GetByIdAsync(product.CategoryId, cancellationToken);

        var dto = MapToDto(product);
        var response = new ProductDetailResponse(dto, category?.Name, category?.Slug);

        return Result<ProductDetailResponse>.Success(response);
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> GetFeaturedAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetFeaturedAsync(count, cancellationToken);
        var dtos = products.Select(MapToDto).ToList();
        return Result<IReadOnlyList<ProductDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var categoryExists = await _categoryRepository.ExistsAsync(categoryId, cancellationToken);
        if (!categoryExists)
            return Result<IReadOnlyList<ProductDto>>.Failure("Category not found", "CATEGORY_NOT_FOUND");

        var products = await _productRepository.GetByCategoryAsync(categoryId, cancellationToken);
        var dtos = products.Select(MapToDto).ToList();
        return Result<IReadOnlyList<ProductDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Result<IReadOnlyList<ProductDto>>.Failure("Search term is required", "INVALID_SEARCH_TERM");

        var products = await _productRepository.SearchAsync(searchTerm, cancellationToken);
        var dtos = products.Select(MapToDto).ToList();
        return Result<IReadOnlyList<ProductDto>>.Success(dtos);
    }

    public async Task<Result<ProductDto>> CreateAsync(Guid vendorId, CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId, cancellationToken);
        if (!categoryExists)
            return Result<ProductDto>.Failure("Category not found", "CATEGORY_NOT_FOUND");

        var skuExists = await _productRepository.GetBySKUAsync(request.SKU, cancellationToken);
        if (skuExists != null)
            return Result<ProductDto>.Failure("A product with this SKU already exists", "PRODUCT_SKU_EXISTS");

        var slugExists = await _productRepository.GetBySlugAsync(request.Slug, cancellationToken);
        if (slugExists != null)
            return Result<ProductDto>.Failure("A product with this slug already exists", "PRODUCT_SLUG_EXISTS");

        var product = Product.Create(
            vendorId,
            request.CategoryId,
            request.Name,
            request.Slug,
            request.Price,
            request.SKU,
            request.StockQuantity,
            request.Description,
            request.CompareAtPrice,
            request.LowStockThreshold,
            request.IsFeatured
        );

        await _productRepository.AddAsync(product, cancellationToken);

        return Result<ProductDto>.Success(MapToDto(product));
    }

    public async Task<Result<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null)
            return Result<ProductDto>.Failure("Product not found", "PRODUCT_NOT_FOUND");

        product.Update(
            request.Name ?? product.Name,
            request.Description,
            request.Price,
            request.CompareAtPrice,
            request.LowStockThreshold,
            request.IsFeatured
        );

        await _productRepository.UpdateAsync(product, cancellationToken);

        return Result<ProductDto>.Success(MapToDto(product));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null)
            return Result.Failure("Product not found", "PRODUCT_NOT_FOUND");

        await _productRepository.DeleteAsync(product, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ToggleActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null)
            return Result.Failure("Product not found", "PRODUCT_NOT_FOUND");

        if (product.IsActive)
            product.Deactivate();
        else
            product.Activate();

        await _productRepository.UpdateAsync(product, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ToggleFeaturedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null)
            return Result.Failure("Product not found", "PRODUCT_NOT_FOUND");

        if (product.IsFeatured)
            product.RemoveFromFeatured();
        else
            product.SetAsFeatured();

        await _productRepository.UpdateAsync(product, cancellationToken);

        return Result.Success();
    }

    public async Task<Result<ProductDto>> AddImageAsync(Guid productId, string imageUrl, string? altText, int displayOrder, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product == null)
            return Result<ProductDto>.Failure("Product not found", "PRODUCT_NOT_FOUND");

        if (string.IsNullOrWhiteSpace(imageUrl))
            return Result<ProductDto>.Failure("Image URL is required", "INVALID_IMAGE_URL");

        product.AddImage(imageUrl, altText, displayOrder);
        await _productRepository.UpdateAsync(product, cancellationToken);

        return Result<ProductDto>.Success(MapToDto(product));
    }

    public async Task<Result> RemoveImageAsync(Guid productId, Guid imageId, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product == null)
            return Result.Failure("Product not found", "PRODUCT_NOT_FOUND");

        product.RemoveImage(imageId);
        await _productRepository.UpdateAsync(product, cancellationToken);

        return Result.Success();
    }

    private static ProductDto MapToDto(Product product)
    {
        var images = product.Images
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new ProductImageDto(i.Id, i.ImageUrl, i.AltText, i.DisplayOrder))
            .ToList();

        return new ProductDto(
            product.Id,
            product.VendorId,
            product.CategoryId,
            product.Name,
            product.Slug,
            product.Description,
            product.Price,
            product.CompareAtPrice,
            product.SKU,
            product.StockQuantity,
            product.LowStockThreshold,
            product.IsFeatured,
            product.IsActive,
            product.ReviewCount,
            product.AverageRating,
            product.IsInStock,
            product.IsLowStock,
            product.HasDiscount,
            product.DiscountPercentage,
            product.MainImageUrl,
            images,
            product.CreatedAt,
            product.UpdatedAt ?? product.CreatedAt
        );
    }

    private static IQueryable<Product> ApplyFilters(IQueryable<Product> queryable, ProductListQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.ToLowerInvariant();
            queryable = queryable.Where(p =>
                p.Name.ToLowerInvariant().Contains(term) ||
                (p.Description != null && p.Description.ToLowerInvariant().Contains(term)));
        }

        if (query.CategoryId.HasValue)
            queryable = queryable.Where(p => p.CategoryId == query.CategoryId.Value);

        if (query.VendorId.HasValue)
            queryable = queryable.Where(p => p.VendorId == query.VendorId.Value);

        if (query.IsFeatured.HasValue)
            queryable = queryable.Where(p => p.IsFeatured == query.IsFeatured.Value);

        if (query.IsActive.HasValue)
            queryable = queryable.Where(p => p.IsActive == query.IsActive.Value);

        if (query.IsInStock.HasValue)
            queryable = queryable.Where(p => p.StockQuantity > 0 == query.IsInStock.Value);

        if (query.MinPrice.HasValue)
            queryable = queryable.Where(p => p.Price >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            queryable = queryable.Where(p => p.Price <= query.MaxPrice.Value);

        return queryable;
    }

    private static IQueryable<Product> ApplySorting(IQueryable<Product> queryable, ProductListQuery query)
    {
        return query.SortBy?.ToLowerInvariant() switch
        {
            "name" => query.SortDescending
                ? queryable.OrderByDescending(p => p.Name)
                : queryable.OrderBy(p => p.Name),
            "price" => query.SortDescending
                ? queryable.OrderByDescending(p => p.Price)
                : queryable.OrderBy(p => p.Price),
            "created" => query.SortDescending
                ? queryable.OrderByDescending(p => p.CreatedAt)
                : queryable.OrderBy(p => p.CreatedAt),
            "rating" => query.SortDescending
                ? queryable.OrderByDescending(p => p.AverageRating)
                : queryable.OrderBy(p => p.AverageRating),
            _ => query.SortDescending
                ? queryable.OrderByDescending(p => p.CreatedAt)
                : queryable.OrderBy(p => p.CreatedAt)
        };
    }
}
