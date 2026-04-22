using ECommerce.Application.Orders.DTOs;
using ECommerce.Application.Products.DTOs;

namespace ECommerce.Application.Admin.DTOs;

public record DashboardSummaryDto(
    int TotalOrders,
    decimal TotalSales,
    int TotalProducts,
    int TotalCustomers, // placeholder; implement user service later if needed
    IEnumerable<OrderDto> RecentOrders,
    IEnumerable<ProductDto> TopSellingProducts,
    IEnumerable<ProductDto> LowStockProducts
);