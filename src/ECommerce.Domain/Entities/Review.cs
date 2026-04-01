namespace ECommerce.Domain.Entities;

public class Review : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Product? Product { get; private set; }
    public Guid UserId { get; private set; }
    public User? User { get; private set; }
    public int Rating { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Comment { get; private set; }
    public bool IsVerifiedPurchase { get; private set; }

    private Review() { }

    public static Review Create(
        Guid productId,
        Guid userId,
        int rating,
        string title,
        string? comment = null,
        bool isVerifiedPurchase = false)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Review title is required", nameof(title));

        return new Review
        {
            ProductId = productId,
            UserId = userId,
            Rating = rating,
            Title = title.Trim(),
            Comment = comment?.Trim(),
            IsVerifiedPurchase = isVerifiedPurchase
        };
    }

    public void Update(int rating, string title, string? comment)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Review title is required", nameof(title));

        Rating = rating;
        Title = title.Trim();
        Comment = comment?.Trim();
        MarkAsUpdated();
    }

    public void SetAsVerifiedPurchase() => IsVerifiedPurchase = true;
}
