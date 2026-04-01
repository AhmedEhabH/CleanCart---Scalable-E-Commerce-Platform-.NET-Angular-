namespace ECommerce.Application.Categories.DTOs;

public record UpdateCategoryRequest(
    string? Name = null,
    string? Description = null,
    string? IconUrl = null,
    int? DisplayOrder = null
);
