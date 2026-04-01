using ECommerce.Api.Models;
using ECommerce.Application.Categories.DTOs;
using ECommerce.Application.Categories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.Api.Controllers;

/// <summary>
/// Category management endpoints for browsing and managing product categories
/// </summary>
[Authorize]
[EnableRateLimiting("categories")]
public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Get all categories in a hierarchical tree structure
    /// </summary>
    /// <returns>List of categories with nested children</returns>
    /// <response code="200">Returns the category tree</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CategoryDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? HandleSuccess(result.Value) : HandleBadRequest(result.Error ?? "Bad request");
    }

    /// <summary>
    /// Get a specific category by its ID
    /// </summary>
    /// <param name="id">Category unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Category details with nested children</returns>
    /// <response code="200">Returns the category</response>
    /// <response code="404">Category not found</response>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetByIdAsync(id, cancellationToken);
        if (result.IsFailure)
            return HandleNotFound(result.Error ?? "Resource not found");
        return HandleSuccess(result.Value);
    }

    /// <summary>
    /// Create a new category (Admin only)
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/categories
    ///     {
    ///         "name": "Electronics",
    ///         "slug": "electronics",
    ///         "description": "Electronic devices and accessories",
    ///         "displayOrder": 1
    ///     }
    /// </remarks>
    /// <param name="request">Category creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created category</returns>
    /// <response code="201">Category created successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.CreateAsync(request, cancellationToken);
        if (result.IsFailure)
            return HandleBadRequest(result.Error ?? "Bad request");
        return HandleCreated(result.Value);
    }

    /// <summary>
    /// Update an existing category (Admin only)
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/categories/{id}
    ///     {
    ///         "name": "Updated Electronics",
    ///         "description": "Updated description"
    ///     }
    /// </remarks>
    /// <param name="id">Category unique identifier</param>
    /// <param name="request">Fields to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated category</returns>
    /// <response code="200">Category updated successfully</response>
    /// <response code="403">Insufficient permissions</response>
    /// <response code="404">Category not found</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.UpdateAsync(id, request, cancellationToken);
        if (result.IsFailure)
            return HandleNotFound(result.Error ?? "Resource not found");
        return HandleSuccess(result.Value);
    }

    /// <summary>
    /// Delete a category by its ID (Admin only)
    /// </summary>
    /// <param name="id">Category unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Category deleted successfully</response>
    /// <response code="403">Insufficient permissions</response>
    /// <response code="404">Category not found</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.DeleteAsync(id, cancellationToken);
        if (result.IsFailure)
            return HandleNotFound(result.Error ?? "Resource not found");
        return HandleOkWithMessage("Category deleted successfully");
    }
}
