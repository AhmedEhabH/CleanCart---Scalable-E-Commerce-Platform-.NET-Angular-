using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public Role Role { get; private set; }
    public bool EmailConfirmed { get; private set; }

    private readonly List<Address> _addresses = new();
    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();

    private readonly List<Cart> _carts = new();
    public IReadOnlyCollection<Cart> Carts => _carts.AsReadOnly();

    private readonly List<Wishlist> _wishlists = new();
    public IReadOnlyCollection<Wishlist> Wishlists => _wishlists.AsReadOnly();

    private readonly List<Order> _orders = new();
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    private readonly List<Review> _reviews = new();
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();

    private readonly List<Vendor> _vendors = new();
    public IReadOnlyCollection<Vendor> Vendors => _vendors.AsReadOnly();

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { }

    public static User Create(string email, string passwordHash, string firstName, string lastName, string? phoneNumber = null, Role role = Role.User)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
        if (!email.Contains('@'))
            throw new ArgumentException("Invalid email format", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password is required", nameof(passwordHash));
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        return new User
        {
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            PhoneNumber = phoneNumber?.Trim(),
            Role = role
        };
    }

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PhoneNumber = phoneNumber?.Trim();
        MarkAsUpdated();
    }

    public void SetPassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required", nameof(passwordHash));

        PasswordHash = passwordHash;
        MarkAsUpdated();
    }

    public void ConfirmEmail() => EmailConfirmed = true;

    public string FullName => $"{FirstName} {LastName}";
}
