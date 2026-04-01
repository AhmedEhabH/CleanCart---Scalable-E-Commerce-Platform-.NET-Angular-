using ECommerce.Api.Controllers;
using ECommerce.Api.Models;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Orders.DTOs;
using ECommerce.Application.Orders.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

namespace ECommerce.Api.Tests.Controllers;

public class CheckoutControllerTests
{
    private readonly Mock<IOrderService> _mockOrderService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly CheckoutController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public CheckoutControllerTests()
    {
        _mockOrderService = new Mock<IOrderService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockCurrentUserService.Setup(x => x.UserId).Returns(_userId);
        _controller = new CheckoutController(_mockOrderService.Object, _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task Checkout_ShouldReturnCreated_WhenCheckoutIsSuccessful()
    {
        var request = CreateValidOrderRequest();
        var orderDto = CreateOrderDto();

        _mockOrderService
            .Setup(x => x.CreateOrderAsync(_userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDto>.Success(orderDto));

        var result = await _controller.Checkout(request, CancellationToken.None);

        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        _mockOrderService.Verify(x => x.CreateOrderAsync(_userId, request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Checkout_ShouldReturnBadRequest_WhenCartIsEmpty()
    {
        var request = CreateValidOrderRequest();

        _mockOrderService
            .Setup(x => x.CreateOrderAsync(_userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDto>.Failure("Cart is empty"));

        var result = await _controller.Checkout(request, CancellationToken.None);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Checkout_ShouldReturnBadRequest_WhenInsufficientStock()
    {
        var request = CreateValidOrderRequest();

        _mockOrderService
            .Setup(x => x.CreateOrderAsync(_userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDto>.Failure("Insufficient stock for 'Product'. Available: 5, Requested: 10"));

        var result = await _controller.Checkout(request, CancellationToken.None);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Checkout_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(x => x.UserId).Returns((Guid?)null);
        var controller = new CheckoutController(_mockOrderService.Object, mockCurrentUserService.Object);

        var request = CreateValidOrderRequest();
        var result = await controller.Checkout(request, CancellationToken.None);

        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    private static CreateOrderRequest CreateValidOrderRequest()
    {
        return new CreateOrderRequest(
            new AddressDto("123 Main St", "New York", "NY", "10001", "USA"),
            new AddressDto("123 Main St", "New York", "NY", "10001", "USA"),
            "Please deliver after 5pm"
        );
    }

    private static OrderDto CreateOrderDto()
    {
        return new OrderDto(
            Guid.NewGuid(),
            "ORD-20240101-ABC12345",
            "Pending",
            100m,
            10m,
            0m,
            0m,
            110m,
            new AddressDto("123 Main St", "New York", "NY", "10001", "USA"),
            new AddressDto("123 Main St", "New York", "NY", "10001", "USA"),
            "Please deliver after 5pm",
            DateTime.UtcNow,
            null,
            new List<OrderItemDto>(),
            2,
            null,
            null
        );
    }
}
