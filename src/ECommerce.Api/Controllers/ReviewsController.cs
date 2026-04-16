using ECommerce.Api.Models;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Reviews.DTOs;
using ECommerce.Application.Reviews.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.Api.Controllers;

[Authorize]
[EnableRateLimiting("products")]
public class ReviewsController : BaseApiController
{
    private readonly IReviewService _reviewService;
    private readonly ICurrentUserService _currentUserService;

    public ReviewsController(IReviewService reviewService, ICurrentUserService currentUserService)
    {
        _reviewService = reviewService;
        _currentUserService = currentUserService;
    }

    [HttpGet("product/{productId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ReviewDto>>), 200)]
    public async Task<IActionResult> GetByProductId(Guid productId, CancellationToken cancellationToken)
    {
        var result = await _reviewService.GetByProductIdAsync(productId, cancellationToken);
        if (result.IsFailure)
            return HandleBadRequest(result.Error ?? "Failed to get reviews");
        return HandleSuccess(result.Value);
    }

    [HttpGet("product/{productId:guid}/summary")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ReviewSummaryDto>), 200)]
    public async Task<IActionResult> GetProductSummary(Guid productId, CancellationToken cancellationToken)
    {
        var result = await _reviewService.GetProductReviewSummaryAsync(productId, cancellationToken);
        if (result.IsFailure)
            return HandleBadRequest(result.Error ?? "Failed to get review summary");
        return HandleSuccess(result.Value);
    }

    [HttpGet("{reviewId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ReviewDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(Guid reviewId, CancellationToken cancellationToken)
    {
        var result = await _reviewService.GetByIdAsync(reviewId, cancellationToken);
        if (result.IsFailure)
            return HandleNotFound(result.Error ?? "Review not found");
        return HandleSuccess(result.Value);
    }

    [HttpGet("product/{productId:guid}/has-purchased")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<IActionResult> HasUserPurchasedProduct(Guid productId, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid userId)
            return HandleUnauthorized("User not authenticated");

        var result = await _reviewService.HasUserPurchasedProductAsync(userId, productId, cancellationToken);
        if (result.IsFailure)
            return HandleBadRequest(result.Error ?? "Failed to check purchase status");
        return HandleSuccess(result.Value);
    }

    [HttpPost("product/{productId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ReviewDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> Create(Guid productId, [FromBody] CreateReviewRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid userId)
            return HandleUnauthorized("User not authenticated");

        var result = await _reviewService.CreateAsync(productId, userId, request, cancellationToken);
        if (result.IsFailure)
            return HandleBadRequest(result.Error ?? "Failed to create review");
        return HandleCreated(result.Value);
    }

    [HttpPut("{reviewId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ReviewDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Update(Guid reviewId, [FromBody] UpdateReviewRequest request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid userId)
            return HandleUnauthorized("User not authenticated");

        var result = await _reviewService.UpdateAsync(reviewId, userId, request, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error?.Contains("not found") == true)
                return HandleNotFound(result.Error);
            return HandleBadRequest(result.Error ?? "Failed to update review");
        }
        return HandleSuccess(result.Value);
    }

    [HttpDelete("{reviewId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Delete(Guid reviewId, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid userId)
            return HandleUnauthorized("User not authenticated");

        var result = await _reviewService.DeleteAsync(reviewId, userId, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error?.Contains("not found") == true)
                return HandleNotFound(result.Error);
            return HandleBadRequest(result.Error ?? "Failed to delete review");
        }
        return HandleOkWithMessage("Review deleted successfully");
    }
}
