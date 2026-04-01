namespace ECommerce.Domain.Entities;

public class Vendor : BaseEntity
{
    public Guid UserId { get; private set; }
    public string BusinessName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public bool IsApproved { get; private set; }
    public DateTime? ApprovedAt { get; private set; }

    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    private Vendor() { }

    public static Vendor Create(
        Guid userId,
        string businessName,
        string? description = null,
        string? logoUrl = null,
        string? contactEmail = null,
        string? contactPhone = null)
    {
        if (string.IsNullOrWhiteSpace(businessName))
            throw new ArgumentException("Business name is required", nameof(businessName));
        if (string.IsNullOrWhiteSpace(contactEmail))
            throw new ArgumentException("Contact email is required", nameof(contactEmail));

        return new Vendor
        {
            UserId = userId,
            BusinessName = businessName.Trim(),
            Description = description?.Trim(),
            LogoUrl = logoUrl,
            ContactEmail = contactEmail.Trim().ToLowerInvariant(),
            ContactPhone = contactPhone?.Trim(),
            IsApproved = false
        };
    }

    public void Approve()
    {
        IsApproved = true;
        ApprovedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Reject()
    {
        IsApproved = false;
        ApprovedAt = null;
        MarkAsUpdated();
    }

    public void Update(string businessName, string? description, string? logoUrl, string? contactEmail, string? contactPhone)
    {
        BusinessName = string.IsNullOrWhiteSpace(businessName) 
            ? BusinessName 
            : businessName.Trim();
        Description = description?.Trim() ?? Description;
        LogoUrl = logoUrl ?? LogoUrl;
        ContactEmail = string.IsNullOrWhiteSpace(contactEmail) 
            ? ContactEmail 
            : contactEmail.Trim().ToLowerInvariant();
        ContactPhone = contactPhone?.Trim() ?? ContactPhone;
        MarkAsUpdated();
    }

    public string DisplayName => BusinessName;
}
