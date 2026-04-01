namespace ECommerce.Domain.Entities;

public class Cart : BaseEntity
{
    public Guid UserId { get; private set; }
    public User? User { get; private set; }
    public string? SessionId { get; private set; }

    private readonly List<CartItem> _items = new();
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    private Cart() { }

    public static Cart CreateForUser(Guid userId)
    {
        return new Cart
        {
            UserId = userId,
            IsActive = true
        };
    }

    public static Cart CreateForSession(string sessionId)
    {
        return new Cart
        {
            SessionId = sessionId,
            IsActive = true
        };
    }

    public void SetSession(string sessionId)
    {
        SessionId = sessionId;
        MarkAsUpdated();
    }

    public void TransferToUser(Guid userId)
    {
        UserId = userId;
        SessionId = null;
        MarkAsUpdated();
    }

    public CartItem AddItem(Guid productId, int quantity)
    {
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            return existingItem;
        }

        var item = CartItem.Create(Id, productId, quantity);
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

    public void UpdateItemQuantity(Guid itemId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new InvalidOperationException("Cart item not found");
        
        if (quantity <= 0)
        {
            RemoveItem(itemId);
        }
        else
        {
            item.UpdateQuantity(quantity);
            MarkAsUpdated();
        }
    }

    public void Clear()
    {
        _items.Clear();
        MarkAsUpdated();
    }

    public decimal SubTotal => _items.Sum(i => i.Total);

    public int TotalItems => _items.Sum(i => i.Quantity);

    public bool IsEmpty => !_items.Any();

    public bool HasProduct(Guid productId) => _items.Any(i => i.ProductId == productId);
}

public class CartItem : BaseEntity
{
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public Product? Product { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private CartItem() { }

    public static CartItem Create(Guid cartId, Guid productId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        return new CartItem
        {
            CartId = cartId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = 0
        };
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        
        Quantity = quantity;
        MarkAsUpdated();
    }

    public void SetUnitPrice(decimal price)
    {
        UnitPrice = Math.Round(price, 2);
    }

    public decimal Total => UnitPrice * Quantity;
}
