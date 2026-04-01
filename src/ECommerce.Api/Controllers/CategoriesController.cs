using ECommerce.Application.Categories.DTOs;
using ECommerce.Application.Categories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? HandleSuccess(result.Value) : HandleBadRequest(result.Error ?? "Bad request");
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetByIdAsync(id, cancellationToken);
        if (result.IsFailure)
            return HandleNotFound(result.Error ?? "Resource not found");
        return HandleSuccess(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.CreateAsync(request, cancellationToken);
        if (result.IsFailure)
            return HandleBadRequest(result.Error ?? "Bad request");
        return HandleCreated(result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.UpdateAsync(id, request, cancellationToken);
        if (result.IsFailure)
            return HandleNotFound(result.Error ?? "Resource not found");
        return HandleSuccess(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.DeleteAsync(id, cancellationToken);
        if (result.IsFailure)
            return HandleNotFound(result.Error ?? "Resource not found");
        return HandleNoContent();
    }
}
