namespace ECommerce.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ParentId { get; private set; }
    public Category? Parent { get; private set; }
    public string? IconUrl { get; private set; }
    public int DisplayOrder { get; private set; }

    private readonly List<Category> _children = new();
    public IReadOnlyCollection<Category> Children => _children.AsReadOnly();

    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    private Category() { }

    public static Category Create(
        string name,
        string slug,
        string? description = null,
        Guid? parentId = null,
        string? iconUrl = null,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Category slug is required", nameof(slug));

        return new Category
        {
            Name = name.Trim(),
            Slug = slug.ToLowerInvariant().Trim(),
            Description = description?.Trim(),
            ParentId = parentId,
            IconUrl = iconUrl,
            DisplayOrder = displayOrder
        };
    }

    public void Update(string name, string? description, string? iconUrl, int displayOrder)
    {
        Name = string.IsNullOrWhiteSpace(name) ? Name : name.Trim();
        Description = description?.Trim() ?? Description;
        IconUrl = iconUrl ?? IconUrl;
        DisplayOrder = displayOrder;
        MarkAsUpdated();
    }

    public void SetAsRoot()
    {
        ParentId = null;
        MarkAsUpdated();
    }

    public void SetParent(Guid parentId)
    {
        if (Id == parentId)
            throw new InvalidOperationException("Category cannot be its own parent");
        ParentId = parentId;
        MarkAsUpdated();
    }

    public string FullPath => Parent != null ? $"{Parent.FullPath} > {Name}" : Name;
}
