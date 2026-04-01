namespace ECommerce.Application.Categories.DTOs;

public record CreateCategoryRequest(
    string Name,
    string Slug,
    string? Description = null,
    Guid? ParentId = null,
    string? IconUrl = null,
    int DisplayOrder = 0
);
