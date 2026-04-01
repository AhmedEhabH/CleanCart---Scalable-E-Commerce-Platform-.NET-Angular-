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

public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _mockOrderService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly OrdersController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public OrdersControllerTests()
    {
        _mockOrderService = new Mock<IOrderService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockCurrentUserService.Setup(x => x.UserId).Returns(_userId);
        _controller = new OrdersController(_mockOrderService.Object, _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnCreated_WhenOrderIsSuccessful()
    {
        var request = CreateValidOrderRequest();
        var orderDto = CreateOrderDto();

        _mockOrderService
            .Setup(x => x.CreateOrderAsync(_userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDto>.Success(orderDto));

        var result = await _controller.CreateOrder(request, CancellationToken.None);

        var createdResult = result.Should().BeOfType<ObjectResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        _mockOrderService.Verify(x => x.CreateOrderAsync(_userId, request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenServiceFails()
    {
        var request = CreateValidOrderRequest();

        _mockOrderService
            .Setup(x => x.CreateOrderAsync(_userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDto>.Failure("Cart is empty"));

        var result = await _controller.CreateOrder(request, CancellationToken.None);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(x => x.UserId).Returns((Guid?)null);
        var controller = new OrdersController(_mockOrderService.Object, mockCurrentUserService.Object);

        var request = CreateValidOrderRequest();
        var result = await controller.CreateOrder(request, CancellationToken.None);

        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task GetMyOrders_ShouldReturnOk_WhenOrdersExist()
    {
        var orders = new List<OrderDto> { CreateOrderDto(), CreateOrderDto() };

        _mockOrderService
            .Setup(x => x.GetUserOrdersAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<OrderDto>>.Success(orders));

        var result = await _controller.GetMyOrders(CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        _mockOrderService.Verify(x => x.GetUserOrdersAsync(_userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMyOrders_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(x => x.UserId).Returns((Guid?)null);
        var controller = new OrdersController(_mockOrderService.Object, mockCurrentUserService.Object);

        var result = await controller.GetMyOrders(CancellationToken.None);

        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task GetOrderDetails_ShouldReturnOk_WhenOrderExists()
    {
        var orderId = Guid.NewGuid();
        var orderDto = CreateOrderDto();

        _mockOrderService
            .Setup(x => x.GetOrderByIdAsync(orderId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDto>.Success(orderDto));

        var result = await _controller.GetOrderDetails(orderId, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        _mockOrderService.Verify(x => x.GetOrderByIdAsync(orderId, _userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrderDetails_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        var orderId = Guid.NewGuid();

        _mockOrderService
            .Setup(x => x.GetOrderByIdAsync(orderId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDto>.Failure("Order not found"));

        var result = await _controller.GetOrderDetails(orderId, CancellationToken.None);

        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetOrderDetails_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(x => x.UserId).Returns((Guid?)null);
        var controller = new OrdersController(_mockOrderService.Object, mockCurrentUserService.Object);

        var result = await controller.GetOrderDetails(Guid.NewGuid(), CancellationToken.None);

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
