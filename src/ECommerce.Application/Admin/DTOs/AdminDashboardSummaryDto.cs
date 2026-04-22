using ECommerce.Application.Orders.DTOs;
using ECommerce.Application.Products.DTOs;

namespace ECommerce.Application.Admin.DTOs;

public class AdminDashboardSummaryDto(
    int totalOrders,
    decimal totalSales,
    int totalProducts,
    int totalUsers,
    List<OrderDto> recentOrders,
    List<ProductDto> topProducts,
    List<ProductDto> lowStockProducts)
{
    public int TotalOrders { get; init; } = totalOrders;
    public decimal TotalSales { get; init; } = totalSales;
    public int TotalProducts { get; init; } = totalProducts;
    public int TotalUsers { get; init; } = totalUsers;

    public List<OrderDto> RecentOrders { get; init; } = recentOrders;
    public List<ProductDto> TopProducts { get; init; } = topProducts;
    public List<ProductDto> LowStockProducts { get; init; } = lowStockProducts;
}