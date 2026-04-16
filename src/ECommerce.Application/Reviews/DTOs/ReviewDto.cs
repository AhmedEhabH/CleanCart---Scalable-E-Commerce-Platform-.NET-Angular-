namespace ECommerce.Application.Reviews.DTOs;

public record ReviewDto(
    Guid Id,
    Guid ProductId,
    Guid UserId,
    string UserName,
    int Rating,
    string Title,
    string? Comment,
    bool IsVerifiedPurchase,
    DateTime CreatedAt
);

public record CreateReviewRequest(
    int Rating,
    string Title,
    string? Comment
);

public record UpdateReviewRequest(
    int Rating,
    string Title,
    string? Comment
);

public record ReviewSummaryDto(
    int TotalReviews,
    decimal AverageRating,
    int OneStar,
    int TwoStars,
    int ThreeStars,
    int FourStars,
    int FiveStars
);
