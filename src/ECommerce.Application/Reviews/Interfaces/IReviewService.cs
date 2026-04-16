using ECommerce.Application.Common.Models;
using ECommerce.Application.Reviews.DTOs;

namespace ECommerce.Application.Reviews.Interfaces;

public interface IReviewService
{
    Task<Result<ReviewDto>> CreateAsync(Guid productId, Guid userId, CreateReviewRequest request, CancellationToken cancellationToken = default);
    Task<Result<ReviewDto>> UpdateAsync(Guid reviewId, Guid userId, UpdateReviewRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default);
    Task<Result<ReviewDto>> GetByIdAsync(Guid reviewId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ReviewDto>>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<Result<ReviewSummaryDto>> GetProductReviewSummaryAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<Result<bool>> HasUserPurchasedProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
}
