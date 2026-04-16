using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Reviews.DTOs;
using ECommerce.Application.Reviews.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class ReviewService : IReviewService
{
    private readonly IApplicationDbContext _context;
    private readonly IReviewRepository _reviewRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILoggerService _logger;

    public ReviewService(
        IApplicationDbContext context,
        IReviewRepository reviewRepository,
        IProductRepository productRepository,
        ILoggerService logger)
    {
        _context = context;
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Result<ReviewDto>> CreateAsync(
        Guid productId,
        Guid userId,
        CreateReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Failed to create review: product {ProductId} not found", productId);
            return Result<ReviewDto>.Failure("Product not found", "PRODUCT_NOT_FOUND");
        }

        var existingReviews = await _reviewRepository.GetByProductIdAsync(productId, cancellationToken);
        if (existingReviews.Any(r => r.UserId == userId))
        {
            _logger.LogWarning("User {UserId} already reviewed product {ProductId}", userId, productId);
            return Result<ReviewDto>.Failure("You have already reviewed this product", "REVIEW_EXISTS");
        }

        var isVerifiedPurchase = await HasUserPurchasedProductInternalAsync(userId, productId, cancellationToken);

        var review = Review.Create(
            productId,
            userId,
            request.Rating,
            request.Title,
            request.Comment,
            isVerifiedPurchase
        );

        var created = await _reviewRepository.AddAsync(review, cancellationToken);

        await UpdateProductRatingAsync(productId, cancellationToken);

        _logger.LogInformation("Review created: {ReviewId} for product {ProductId} by user {UserId}", 
            created.Id, productId, userId);

        var fullReview = await _reviewRepository.GetByIdAsync(created.Id, cancellationToken);
        return Result<ReviewDto>.Success(MapToDto(fullReview!));
    }

    public async Task<Result<ReviewDto>> UpdateAsync(
        Guid reviewId,
        Guid userId,
        UpdateReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);
        if (review == null)
        {
            _logger.LogWarning("Failed to update review: {ReviewId} not found", reviewId);
            return Result<ReviewDto>.Failure("Review not found", "REVIEW_NOT_FOUND");
        }

        if (review.UserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to update review {ReviewId} owned by {OwnerId}", 
                userId, reviewId, review.UserId);
            return Result<ReviewDto>.Failure("You can only update your own reviews", "NOT_AUTHORIZED");
        }

        review.Update(request.Rating, request.Title, request.Comment);
        await _reviewRepository.UpdateAsync(review, cancellationToken);

        await UpdateProductRatingAsync(review.ProductId, cancellationToken);

        _logger.LogInformation("Review updated: {ReviewId} by user {UserId}", reviewId, userId);

        var updatedReview = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);
        return Result<ReviewDto>.Success(MapToDto(updatedReview!));
    }

    public async Task<Result> DeleteAsync(
        Guid reviewId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);
        if (review == null)
        {
            _logger.LogWarning("Failed to delete review: {ReviewId} not found", reviewId);
            return Result.Failure("Review not found", "REVIEW_NOT_FOUND");
        }

        if (review.UserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to delete review {ReviewId} owned by {OwnerId}", 
                userId, reviewId, review.UserId);
            return Result.Failure("You can only delete your own reviews", "NOT_AUTHORIZED");
        }

        var productId = review.ProductId;
        await _reviewRepository.DeleteAsync(review, cancellationToken);

        await UpdateProductRatingAsync(productId, cancellationToken);

        _logger.LogInformation("Review deleted: {ReviewId} by user {UserId}", reviewId, userId);

        return Result.Success();
    }

    public async Task<Result<ReviewDto>> GetByIdAsync(
        Guid reviewId,
        CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);
        if (review == null)
        {
            _logger.LogWarning("Review not found: {ReviewId}", reviewId);
            return Result<ReviewDto>.Failure("Review not found", "REVIEW_NOT_FOUND");
        }

        return Result<ReviewDto>.Success(MapToDto(review));
    }

    public async Task<Result<IReadOnlyList<ReviewDto>>> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var reviews = await _reviewRepository.GetByProductIdAsync(productId, cancellationToken);
        var dtos = reviews.Select(MapToDto).ToList();
        return Result<IReadOnlyList<ReviewDto>>.Success(dtos);
    }

    public async Task<Result<ReviewSummaryDto>> GetProductReviewSummaryAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var reviews = await _reviewRepository.GetByProductIdAsync(productId, cancellationToken);
        
        var avgRating = reviews.Count > 0 
            ? (decimal)Math.Round(reviews.Average(r => r.Rating), 1) 
            : 0m;
        
        var summary = new ReviewSummaryDto(
            TotalReviews: reviews.Count,
            AverageRating: avgRating,
            OneStar: reviews.Count(r => r.Rating == 1),
            TwoStars: reviews.Count(r => r.Rating == 2),
            ThreeStars: reviews.Count(r => r.Rating == 3),
            FourStars: reviews.Count(r => r.Rating == 4),
            FiveStars: reviews.Count(r => r.Rating == 5)
        );

        return Result<ReviewSummaryDto>.Success(summary);
    }

    public async Task<Result<bool>> HasUserPurchasedProductAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var hasPurchased = await HasUserPurchasedProductInternalAsync(userId, productId, cancellationToken);
        return Result<bool>.Success(hasPurchased);
    }

    private async Task<bool> HasUserPurchasedProductInternalAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .SelectMany(o => o.Items)
            .AnyAsync(oi => oi.ProductId == productId, cancellationToken);
    }

    private async Task UpdateProductRatingAsync(Guid productId, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetByProductIdAsync(productId, cancellationToken);
        var avgRating = reviews.Count > 0 
            ? (decimal)Math.Round(reviews.Average(r => r.Rating), 1) 
            : 0m;
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product != null)
        {
            product.UpdateRating(avgRating, reviews.Count);
            await _productRepository.UpdateAsync(product, cancellationToken);
        }
    }

    private static ReviewDto MapToDto(Review review)
    {
        return new ReviewDto(
            review.Id,
            review.ProductId,
            review.UserId,
            $"{review.User?.FirstName} {review.User?.LastName}".Trim(),
            review.Rating,
            review.Title,
            review.Comment,
            review.IsVerifiedPurchase,
            review.CreatedAt
        );
    }
}
