namespace ECommerce.Domain.Entities;

public class Wishlist : BaseEntity
{
    public Guid UserId { get; private set; }
    public User? User { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsPublic { get; private set; }

    private readonly List<WishlistItem> _items = new();
    public IReadOnlyCollection<WishlistItem> Items => _items.AsReadOnly();

    private Wishlist() { }

    public static Wishlist Create(Guid userId, string name = "My Wishlist", bool isPublic = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Wishlist name is required", nameof(name));

        return new Wishlist
        {
            UserId = userId,
            Name = name.Trim(),
            IsPublic = isPublic
        };
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Wishlist name is required", nameof(name));
        
        Name = name.Trim();
        MarkAsUpdated();
    }

    public void MakePublic() => IsPublic = true;

    public void MakePrivate() => IsPublic = false;

    public WishlistItem AddItem(Guid productId)
    {
        if (_items.Any(i => i.ProductId == productId))
            throw new InvalidOperationException("Product already in wishlist");

        var item = WishlistItem.Create(Id, productId);
        _items.Add(item);
        MarkAsUpdated();
        return item;
    }

    public void RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            _items.Remove(item);
            MarkAsUpdated();
        }
    }

    public void RemoveItemByProduct(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _items.Remove(item);
            MarkAsUpdated();
        }
    }

    public bool HasProduct(Guid productId) => _items.Any(i => i.ProductId == productId);

    public int TotalItems => _items.Count;
}

public class WishlistItem : BaseEntity
{
    public Guid WishlistId { get; private set; }
    public Guid ProductId { get; private set; }
    public Product? Product { get; private set; }
    public DateTime AddedAt { get; private set; }

    private WishlistItem() { }

    public static WishlistItem Create(Guid wishlistId, Guid productId)
    {
        return new WishlistItem
        {
            WishlistId = wishlistId,
            ProductId = productId,
            AddedAt = DateTime.UtcNow
        };
    }
}
