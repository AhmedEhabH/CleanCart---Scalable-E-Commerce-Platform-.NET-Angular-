using ECommerce.Api.Controllers;
using ECommerce.Api.Models;
using ECommerce.Application.Cart.DTOs;
using ECommerce.Application.Cart.Interfaces;
using ECommerce.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

/// <summary>
/// Shopping cart management endpoints
/// </summary>
[Authorize]
public class CartController : BaseApiController
{
    private readonly ICartService _cartService;
    private readonly ICurrentUserService _currentUserService;

    public CartController(ICartService cartService, ICurrentUserService currentUserService)
    {
        _cartService = cartService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get the current user's shopping cart
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user's cart with all items</returns>
    /// <response code="200">Returns the cart</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid userId)
            return HandleUnauthorized("User not authenticated");

        var result = await _cartService.GetCartAsync(userId, cancellationToken);
        if (result.IsFailure)
            return HandleBadRequest(result.Error ?? "Failed to retrieve cart");
        return HandleSuccess(result.Value);
    }

    /// <summary>
    /// Add a product to the cart
    /// </summary>
    /// <param name="request">Product ID and quantity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated cart</returns>
    /// <response code="200">Product added successfully</response>
    /// <response code="400">Validation error or product already in cart</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("items")]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid userId)
            return HandleUnauthorized("User not authenticated");

        var result = await _cartService.AddItemAsync(userId, request, cancellationToken);
        if (result.IsFailure)
            return HandleBadRequest(result.Error ?? "Failed to add item to cart");
        return HandleSuccess(result.Value);
    }

    /// <summary>
    /// Update the quantity of an item in the cart
    /// </summary>
    /// <param name="itemId">Cart item ID</param>
    /// <param name="request">New quantity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated cart</returns>
    /// <response code="200">Quantity updated successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Item not found</response>
    /// <response code="401">User not authenticated</response>
    [HttpPut("items/{itemId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> UpdateItemQuantity(Guid itemId, [FromBody] UpdateCartItemRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid userId)
            return HandleUnauthorized("User not authenticated");

        var result = await _cartService.UpdateItemQuantityAsync(userId, itemId, request, cancellationToken);
        if (result.IsFailure)
            return HandleNotFound(result.Error ?? "Failed to update item");
        return HandleSuccess(result.Value);
    }

    /// <summary>
    /// Remove an item from the cart
    /// </summary>
    /// <param name="itemId">Cart item ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Item removed successfully</response>
    /// <response code="404">Item not found</response>
    /// <response code="401">User not authenticated</response>
    [HttpDelete("items/{itemId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> RemoveItem(Guid itemId, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid userId)
            return HandleUnauthorized("User not authenticated");

        var result = await _cartService.RemoveItemAsync(userId, itemId, cancellationToken);
        if (result.IsFailure)
            return HandleNotFound(result.Error ?? "Failed to remove item");
        return HandleNoContent();
    }

    /// <summary>
    /// Clear all items from the cart
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Cart cleared successfully</response>
    /// <response code="401">User not authenticated</response>
    [HttpDelete("clear")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> ClearCart(CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid userId)
            return HandleUnauthorized("User not authenticated");

        var result = await _cartService.ClearCartAsync(userId, cancellationToken);
        if (result.IsFailure)
            return HandleBadRequest(result.Error ?? "Failed to clear cart");
        return HandleNoContent();
    }
}
