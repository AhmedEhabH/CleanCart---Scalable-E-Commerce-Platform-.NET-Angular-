using ECommerce.Domain.Enums;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; private set; }
    public User? User { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal ShippingCost { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? ShippingAddressJson { get; private set; }
    public string? BillingAddressJson { get; private set; }
    public string? Notes { get; private set; }
    public Guid? PaymentId { get; private set; }
    public Payment? Payment { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public static Order Create(
        Guid userId,
        decimal subtotal,
        decimal taxAmount,
        decimal shippingCost,
        decimal discountAmount,
        ValueObjects.Address shippingAddress,
        ValueObjects.Address? billingAddress = null,
        string? notes = null)
    {
        if (subtotal < 0)
            throw new ArgumentException("Subtotal cannot be negative", nameof(subtotal));
        if (taxAmount < 0)
            throw new ArgumentException("Tax amount cannot be negative", nameof(taxAmount));
        if (shippingCost < 0)
            throw new ArgumentException("Shipping cost cannot be negative", nameof(shippingCost));
        if (discountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative", nameof(discountAmount));

        var total = subtotal + taxAmount + shippingCost - discountAmount;
        if (total < 0)
            throw new ArgumentException("Total amount cannot be negative");

        return new Order
        {
            UserId = userId,
            OrderNumber = GenerateOrderNumber(),
            Status = OrderStatus.Pending,
            SubTotal = Math.Round(subtotal, 2),
            TaxAmount = Math.Round(taxAmount, 2),
            ShippingCost = Math.Round(shippingCost, 2),
            DiscountAmount = Math.Round(discountAmount, 2),
            TotalAmount = Math.Round(total, 2),
            ShippingAddressJson = System.Text.Json.JsonSerializer.Serialize(shippingAddress),
            BillingAddressJson = billingAddress != null 
                ? System.Text.Json.JsonSerializer.Serialize(billingAddress) 
                : null,
            Notes = notes?.Trim()
        };
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }

    public void AddItem(Guid productId, Guid vendorId, string productName, string sku, decimal price, int quantity, decimal discount = 0)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot add items to non-pending order");

        var item = OrderItem.Create(Id, productId, vendorId, productName, sku, price, quantity, discount);
        _items.Add(item);
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        var validTransitions = GetValidStatusTransitions();
        if (!validTransitions.Contains(newStatus))
            throw new InvalidOperationException($"Cannot transition from {Status} to {newStatus}");

        Status = newStatus;
        MarkAsUpdated();
    }

    private List<OrderStatus> GetValidStatusTransitions()
    {
        return Status switch
        {
            OrderStatus.Pending => new List<OrderStatus> { OrderStatus.Confirmed, OrderStatus.Cancelled },
            OrderStatus.Confirmed => new List<OrderStatus> { OrderStatus.Processing, OrderStatus.Cancelled },
            OrderStatus.Processing => new List<OrderStatus> { OrderStatus.Shipped, OrderStatus.Cancelled },
            OrderStatus.Shipped => new List<OrderStatus> { OrderStatus.Delivered },
            OrderStatus.Delivered => new List<OrderStatus> { OrderStatus.Refunded },
            OrderStatus.Cancelled => new List<OrderStatus>(),
            OrderStatus.Refunded => new List<OrderStatus>(),
            _ => new List<OrderStatus>()
        };
    }

    public void SetPayment(Guid paymentId)
    {
        PaymentId = paymentId;
        MarkAsUpdated();
    }

    public void Confirm() => UpdateStatus(OrderStatus.Confirmed);

    public void StartProcessing() => UpdateStatus(OrderStatus.Processing);

    public void Ship() => UpdateStatus(OrderStatus.Shipped);

    public void Deliver() => UpdateStatus(OrderStatus.Delivered);

    public void Cancel() => UpdateStatus(OrderStatus.Cancelled);

    public void Refund() => UpdateStatus(OrderStatus.Refunded);

    public ValueObjects.Address? GetShippingAddress() => 
        ShippingAddressJson != null 
            ? System.Text.Json.JsonSerializer.Deserialize<ValueObjects.Address>(ShippingAddressJson) 
            : null;

    public ValueObjects.Address? GetBillingAddress() => 
        BillingAddressJson != null 
            ? System.Text.Json.JsonSerializer.Deserialize<ValueObjects.Address>(BillingAddressJson) 
            : null;

    public int TotalItems => _items.Sum(i => i.Quantity);

    public bool IsPaid => Payment != null && Payment.Status == Enums.PaymentStatus.Paid;

    public bool CanBeCancelled => Status == OrderStatus.Pending || Status == OrderStatus.Confirmed;

    public bool IsCompleted => Status == OrderStatus.Delivered;
}

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Product? Product { get; private set; }
    public Guid VendorId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string SKU { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }
    public decimal Discount { get; private set; }

    private OrderItem() { }

    public static OrderItem Create(
        Guid orderId,
        Guid productId,
        Guid vendorId,
        string productName,
        string sku,
        decimal price,
        int quantity,
        decimal discount = 0)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));
        if (discount < 0)
            throw new ArgumentException("Discount cannot be negative", nameof(discount));

        return new OrderItem
        {
            OrderId = orderId,
            ProductId = productId,
            VendorId = vendorId,
            ProductName = productName.Trim(),
            SKU = sku.Trim().ToUpperInvariant(),
            Price = Math.Round(price, 2),
            Quantity = quantity,
            Discount = Math.Round(discount, 2)
        };
    }

    public decimal UnitPrice => Price;

    public decimal Total => (Price - Discount) * Quantity;

    public decimal TotalWithDiscount => Price * Quantity - Discount;
}
