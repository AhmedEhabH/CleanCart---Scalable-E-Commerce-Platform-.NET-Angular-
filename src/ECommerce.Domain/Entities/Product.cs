namespace ECommerce.Domain.Entities;

public class Product : BaseEntity
{
    public Guid? VendorId { get; private set; }
    public Vendor? Vendor { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public decimal? CompareAtPrice { get; private set; }
    public string SKU { get; private set; } = string.Empty;
    public int StockQuantity { get; private set; }
    public int LowStockThreshold { get; private set; } = 10;
    public bool IsFeatured { get; private set; }
    public int ReviewCount { get; private set; }
    public decimal AverageRating { get; private set; }

    private readonly List<ProductImage> _images = new();
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    private readonly List<CartItem> _cartItems = new();
    public IReadOnlyCollection<CartItem> CartItems => _cartItems.AsReadOnly();

    private readonly List<WishlistItem> _wishlistItems = new();
    public IReadOnlyCollection<WishlistItem> WishlistItems => _wishlistItems.AsReadOnly();

    private readonly List<OrderItem> _orderItems = new();
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    private readonly List<Review> _reviews = new();
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();

    private Product() { }

    public static Product Create(
        Guid? vendorId,
        Guid categoryId,
        string name,
        string slug,
        decimal price,
        string sku,
        int stockQuantity,
        string? description = null,
        decimal? compareAtPrice = null,
        int lowStockThreshold = 10,
        bool isFeatured = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Product slug is required", nameof(slug));
        if (price <= 0)
            throw new ArgumentException("Price must be greater than zero", nameof(price));
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required", nameof(sku));
        if (stockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(stockQuantity));

        return new Product
        {
            VendorId = vendorId,
            CategoryId = categoryId,
            Name = name.Trim(),
            Slug = slug.ToLowerInvariant().Trim(),
            Description = description?.Trim(),
            Price = Math.Round(price, 2),
            CompareAtPrice = compareAtPrice.HasValue ? Math.Round(compareAtPrice.Value, 2) : null,
            SKU = sku.Trim().ToUpperInvariant(),
            StockQuantity = stockQuantity,
            LowStockThreshold = lowStockThreshold,
            IsFeatured = isFeatured
        };
    }

    public void Update(
        string name,
        string? description,
        decimal? price,
        decimal? compareAtPrice,
        int? lowStockThreshold,
        bool? isFeatured)
    {
        Name = string.IsNullOrWhiteSpace(name) ? Name : name.Trim();
        Description = description?.Trim() ?? Description;
        Price = price.HasValue ? Math.Round(price.Value, 2) : Price;
        CompareAtPrice = compareAtPrice.HasValue ? Math.Round(compareAtPrice.Value, 2) : CompareAtPrice;
        LowStockThreshold = lowStockThreshold ?? LowStockThreshold;
        IsFeatured = isFeatured ?? IsFeatured;
        MarkAsUpdated();
    }

    public void UpdateCategory(Guid categoryId)
    {
        CategoryId = categoryId;
        MarkAsUpdated();
    }

    public void SetAsFeatured() => IsFeatured = true;

    public void RemoveFromFeatured() => IsFeatured = false;

    public void UpdateStock(int quantity)
    {
        if (StockQuantity + quantity < 0)
            throw new InvalidOperationException("Insufficient stock");
        
        StockQuantity += quantity;
        MarkAsUpdated();
    }

    public void SetStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(quantity));
        
        StockQuantity = quantity;
        MarkAsUpdated();
    }

    public bool IsInStock => StockQuantity > 0;

    public bool IsLowStock => StockQuantity > 0 && StockQuantity <= LowStockThreshold;

    public bool HasDiscount => CompareAtPrice.HasValue && CompareAtPrice.Value > Price;

    public decimal DiscountPercentage => HasDiscount 
        ? Math.Round((CompareAtPrice.Value - Price) / CompareAtPrice.Value * 100, 0) 
        : 0;

    public void AddImage(string imageUrl, string? altText = null, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL is required", nameof(imageUrl));

        _images.Add(ProductImage.Create(Id, imageUrl, altText, displayOrder));
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image != null)
            _images.Remove(image);
    }

    public void UpdateRating(decimal averageRating, int reviewCount)
    {
        AverageRating = Math.Round(averageRating, 1);
        ReviewCount = reviewCount;
        MarkAsUpdated();
    }

    public string? MainImageUrl => _images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl;
}

public class ProductImage : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Product? Product { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public string? AltText { get; private set; }
    public int DisplayOrder { get; private set; }

    private ProductImage() { }

    public static ProductImage Create(Guid productId, string imageUrl, string? altText = null, int displayOrder = 0)
    {
        return new ProductImage
        {
            ProductId = productId,
            ImageUrl = imageUrl.Trim(),
            AltText = altText?.Trim(),
            DisplayOrder = displayOrder
        };
    }

    public void Update(string imageUrl, string? altText, int displayOrder)
    {
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? ImageUrl : imageUrl.Trim();
        AltText = altText?.Trim() ?? AltText;
        DisplayOrder = displayOrder;
        MarkAsUpdated();
    }
}
