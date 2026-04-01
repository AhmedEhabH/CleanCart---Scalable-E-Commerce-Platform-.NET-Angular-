using ECommerce.Application.Categories.DTOs;
using ECommerce.Application.Common.Models;

namespace ECommerce.Application.Categories.Interfaces;

public interface ICategoryService
{
    Task<Result<CategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CategoryDto>>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CategoryDto>>> GetSubcategoriesAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<Result<CategoryDto>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<Result<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
