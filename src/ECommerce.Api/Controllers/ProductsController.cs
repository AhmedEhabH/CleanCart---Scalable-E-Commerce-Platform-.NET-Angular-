using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Products.DTOs;
using ECommerce.Application.Products.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

public class ProductsController : BaseApiController
{
    private readonly IProductService _productService;
    private readonly ICurrentUserService _currentUserService;

    public ProductsController(IProductService productService, ICurrentUserService currentUserService)
    {
        _productService = productService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] ProductListQuery query, CancellationToken cancellationToken)
    {
        var result = await _productService.GetPagedAsync(query, cancellationToken);
        if (result.IsFailure)
            return HandleBadRequest(result.Error ?? "Bad request");
        return HandleSuccess(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.GetByIdAsync(id, cancellationToken);
        if (result.IsFailure)
            return HandleNotFound(result.Error ?? "Resource not found");
        return HandleSuccess(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid vendorId)
            return HandleUnauthorized("User not authenticated");

        var result = await _productService.CreateAsync(vendorId, request, cancellationToken);
        if (result.IsFailure)
            return HandleBadRequest(result.Error ?? "Bad request");
        return HandleCreated(result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.UpdateAsync(id, request, cancellationToken);
        if (result.IsFailure)
            return HandleNotFound(result.Error ?? "Resource not found");
        return HandleSuccess(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.DeleteAsync(id, cancellationToken);
        if (result.IsFailure)
            return HandleNotFound(result.Error ?? "Resource not found");
        return HandleNoContent();
    }
}
