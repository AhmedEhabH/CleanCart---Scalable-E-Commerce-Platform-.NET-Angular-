using ECommerce.Api.Controllers;
using ECommerce.Api.Models;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Wishlist.DTOs;
using ECommerce.Application.Wishlist.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("global")]
public class WishlistController : BaseApiController
{
    private readonly IWishlistService _wishlistService;
    private readonly ICurrentUserService _currentUserService;

    public WishlistController(IWishlistService wishlistService, ICurrentUserService currentUserService)
    {
        _wishlistService = wishlistService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<WishlistItemDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> GetWishlist(CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid userId)
            return HandleUnauthorized("User not authenticated");

        var result = await _wishlistService.GetUserWishlistAsync(userId, cancellationToken);
        if (result.IsFailure)
            return HandleBadRequest(result.Error ?? "Failed to retrieve wishlist");
        return HandleSuccess(result.Value);
    }

    [HttpPost("toggle/{productId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> ToggleItem(Guid productId, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid userId)
            return HandleUnauthorized("User not authenticated");

        var result = await _wishlistService.ToggleWishlistItemAsync(userId, productId, cancellationToken);
        if (result.IsFailure)
            return HandleBadRequest(result.Error ?? "Failed to toggle wishlist item");
        return HandleSuccess(result.Value);
    }

    [HttpPost("sync")]
    [ProducesResponseType(typeof(ApiResponse<List<WishlistItemDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> SyncWishlist([FromBody] List<Guid> localProductIds, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid userId)
            return HandleUnauthorized("User not authenticated");

        var result = await _wishlistService.SyncWishlistAsync(userId, localProductIds, cancellationToken);
        if (result.IsFailure)
            return HandleBadRequest(result.Error ?? "Failed to sync wishlist");
        return HandleSuccess(result.Value);
    }
}