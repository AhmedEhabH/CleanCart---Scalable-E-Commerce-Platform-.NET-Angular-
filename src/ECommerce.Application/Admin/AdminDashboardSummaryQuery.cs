using ECommerce.Application.Admin.DTOs;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Orders.DTOs;
using ECommerce.Application.Orders.Interfaces;
using ECommerce.Application.Products.DTOs;
using ECommerce.Application.Products.Interfaces;
using ECommerce.Application.Users.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Admin;

public record AdminDashboardSummaryQuery : IRequest<AdminDashboardSummaryDto>;

public class AdminDashboardSummaryHandler : IRequestHandler<AdminDashboardSummaryQuery, AdminDashboardSummaryDto>
{
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly IUserService _userService;
    private readonly IApplicationDbContext _context;

    public AdminDashboardSummaryHandler(
        IOrderService orderService,
        IProductService productService,
        IUserService userService,
        IApplicationDbContext context)
    {
        _orderService = orderService;
        _productService = productService;
        _userService = userService;
        _context = context;
    }

    public async Task<AdminDashboardSummaryDto> Handle(AdminDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .ToListAsync(cancellationToken);

        int totalOrders = orders.Count;
        decimal totalSales = orders.Sum(o => o.TotalAmount);

        var products = await _context.Products.ToListAsync(cancellationToken);
        int totalProducts = products.Count;

        int totalUsers = await _userService.GetTotalUsersCountAsync(cancellationToken);

        var recentOrders = orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .Select(o => new OrderDto(
                Id: o.Id,
                OrderNumber: o.OrderNumber,
                Status: o.Status.ToString(),
                SubTotal: o.SubTotal,
                TaxAmount: o.TaxAmount,
                ShippingCost: o.ShippingCost,
                DiscountAmount: o.DiscountAmount,
                TotalAmount: o.TotalAmount,
                ShippingAddress: null,
                BillingAddress: null,
                Notes: o.Notes,
                CreatedAt: o.CreatedAt,
                UpdatedAt: o.UpdatedAt,
                Items: new List<OrderItemDto>(),
                TotalItems: o.TotalItems,
                PaymentId: o.PaymentId,
                PaymentStatus: null))
            .ToList();

        var productSales = orders
            .SelectMany(o => o.Items)
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalQuantitySold = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.Total)
            })
            .OrderByDescending(p => p.TotalQuantitySold)
            .Take(5)
            .ToList();

        var topSellingProductIds = productSales.Select(p => p.ProductId).ToList();
        var topSellingProducts = new List<ProductDto>();
        foreach (var id in topSellingProductIds)
        {
            var productResult = await _productService.GetByIdAsync(id, cancellationToken);
            if (productResult.IsSuccess)
            {
                topSellingProducts.Add(productResult.Value);
            }
        }

        var lowStockProducts = products
            .Where(p => p.StockQuantity < 10)
            .Take(5)
            .Select(p => new ProductDto(
                p.Id,
                p.VendorId,
                p.CategoryId,
                p.Name,
                p.Slug,
                p.Description,
                p.Price,
                p.CompareAtPrice,
                p.SKU,
                p.StockQuantity,
                p.LowStockThreshold,
                p.IsFeatured,
                p.IsActive,
                0,
                0,
                p.StockQuantity > 0,
                p.StockQuantity < p.LowStockThreshold,
                p.CompareAtPrice.HasValue && p.CompareAtPrice > p.Price,
                0,
                null,
                new List<ProductImageDto>(),
                p.CreatedAt,
                p.UpdatedAt ?? DateTime.UtcNow))
            .ToList();

        return new AdminDashboardSummaryDto(
            totalOrders,
            totalSales,
            totalProducts,
            totalUsers,
            recentOrders,
            topSellingProducts,
            lowStockProducts);
    }
}