using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetByRoleAsync(Enums.Role role, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Product?> GetWithImagesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> GetBySKUAsync(string sku, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetFeaturedAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
}

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetSubcategoriesAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> GetProductCountsAsync(CancellationToken cancellationToken = default);
    Task<bool> HasSubcategoriesAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default);
}

public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Cart?> GetWithItemsAsync(Guid cartId, CancellationToken cancellationToken = default);
}

public interface IWishlistRepository : IRepository<Wishlist>
{
    Task<Wishlist?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Wishlist?> GetWithItemsAsync(Guid wishlistId, CancellationToken cancellationToken = default);
}

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByVendorIdAsync(Guid vendorId, CancellationToken cancellationToken = default);
}

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}

public interface IReviewRepository : IRepository<Review>
{
    Task<IReadOnlyList<Review>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Review>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Review> Items, int TotalCount)> GetPagedByProductAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken = default);
}

public interface IVendorRepository : IRepository<Vendor>
{
    Task<Vendor?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vendor>> GetApprovedVendorsAsync(CancellationToken cancellationToken = default);
}
