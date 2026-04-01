namespace ECommerce.Application.Categories.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid? ParentId,
    string? IconUrl,
    bool IsActive,
    int DisplayOrder,
    IReadOnlyList<CategoryDto> Children,
    int ProductCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
