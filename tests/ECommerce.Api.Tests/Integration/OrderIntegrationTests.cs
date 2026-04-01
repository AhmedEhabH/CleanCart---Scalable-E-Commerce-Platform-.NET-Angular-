using ECommerce.Application.Cart.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Orders.DTOs;
using ECommerce.Application.Orders.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using FluentAssertions;
using Moq;

namespace ECommerce.Api.Tests.Integration;

public class OrderIntegrationTests
{
    private static readonly Guid TestUserId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateOrder_ShouldFail_WhenCartIsEmpty()
    {
        using var context = CreateContext();
        var mockCartService = new Mock<ICartService>();
        var orderService = new OrderService(context, mockCartService.Object);

        var result = await orderService.CreateOrderAsync(TestUserId, CreateValidOrderRequest());

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserOrders_ShouldReturnEmpty_WhenNoOrders()
    {
        using var context = CreateContext();
        var mockCartService = new Mock<ICartService>();
        var orderService = new OrderService(context, mockCartService.Object);

        var result = await orderService.GetUserOrdersAsync(TestUserId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOrderById_ShouldFail_WhenOrderNotFound()
    {
        using var context = CreateContext();
        var mockCartService = new Mock<ICartService>();
        var orderService = new OrderService(context, mockCartService.Object);

        var result = await orderService.GetOrderByIdAsync(Guid.NewGuid(), TestUserId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    private static CreateOrderRequest CreateValidOrderRequest()
    {
        return new CreateOrderRequest(
            ShippingAddress: new AddressDto("123 Main St", "New York", "NY", "10001", "USA"),
            BillingAddress: new AddressDto("123 Main St", "New York", "NY", "10001", "USA"),
            Notes: "Test order"
        );
    }
}