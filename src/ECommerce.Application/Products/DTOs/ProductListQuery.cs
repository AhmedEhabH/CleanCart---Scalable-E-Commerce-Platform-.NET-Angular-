namespace ECommerce.Application.Products.DTOs;

public record ProductListQuery(
    int Page = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    Guid? CategoryId = null,
    Guid? VendorId = null,
    bool? IsFeatured = null,
    bool? IsActive = null,
    bool? IsInStock = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? SortBy = null,
    bool SortDescending = false
);
