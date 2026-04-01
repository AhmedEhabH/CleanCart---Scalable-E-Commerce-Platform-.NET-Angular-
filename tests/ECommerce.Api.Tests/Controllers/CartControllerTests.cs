using ECommerce.Api.Controllers;
using ECommerce.Api.Models;
using ECommerce.Application.Cart.DTOs;
using ECommerce.Application.Cart.Interfaces;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

namespace ECommerce.Api.Tests.Controllers;

public class CartControllerTests
{
    private readonly Mock<ICartService> _mockCartService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly CartController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public CartControllerTests()
    {
        _mockCartService = new Mock<ICartService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockCurrentUserService.Setup(x => x.UserId).Returns(_userId);
        _controller = new CartController(_mockCartService.Object, _mockCurrentUserService.Object);
    }

    #region GetCart Tests

    [Fact]
    public async Task GetCart_ShouldReturnOk_WhenCartExists()
    {
        var cartDto = CreateCartDto();

        _mockCartService
            .Setup(x => x.GetCartAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CartDto>.Success(cartDto));

        var result = await _controller.GetCart(CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        _mockCartService.Verify(x => x.GetCartAsync(_userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCart_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(x => x.UserId).Returns((Guid?)null);
        var controller = new CartController(_mockCartService.Object, mockCurrentUserService.Object);

        var result = await controller.GetCart(CancellationToken.None);

        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task GetCart_ShouldReturnBadRequest_WhenServiceFails()
    {
        _mockCartService
            .Setup(x => x.GetCartAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CartDto>.Failure("Failed to retrieve cart"));

        var result = await _controller.GetCart(CancellationToken.None);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    #endregion

    #region AddItem Tests

    [Fact]
    public async Task AddItem_ShouldReturnOk_WhenItemIsAdded()
    {
        var request = new AddCartItemRequest(Guid.NewGuid(), 2);
        var cartDto = CreateCartDto();

        _mockCartService
            .Setup(x => x.AddItemAsync(_userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CartDto>.Success(cartDto));

        var result = await _controller.AddItem(request, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task AddItem_ShouldReturnBadRequest_WhenProductNotFound()
    {
        var request = new AddCartItemRequest(Guid.NewGuid(), 1);

        _mockCartService
            .Setup(x => x.AddItemAsync(_userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CartDto>.Failure("Product not found"));

        var result = await _controller.AddItem(request, CancellationToken.None);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task AddItem_ShouldReturnBadRequest_WhenProductAlreadyInCart()
    {
        var request = new AddCartItemRequest(Guid.NewGuid(), 1);

        _mockCartService
            .Setup(x => x.AddItemAsync(_userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CartDto>.Failure("Product already exists in cart. Use update to change quantity."));

        var result = await _controller.AddItem(request, CancellationToken.None);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task AddItem_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(x => x.UserId).Returns((Guid?)null);
        var controller = new CartController(_mockCartService.Object, mockCurrentUserService.Object);

        var request = new AddCartItemRequest(Guid.NewGuid(), 1);
        var result = await controller.AddItem(request, CancellationToken.None);

        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    #endregion

    #region UpdateItemQuantity Tests

    [Fact]
    public async Task UpdateItemQuantity_ShouldReturnOk_WhenQuantityIsUpdated()
    {
        var itemId = Guid.NewGuid();
        var request = new UpdateCartItemRequest(5);
        var cartDto = CreateCartDto();

        _mockCartService
            .Setup(x => x.UpdateItemQuantityAsync(_userId, itemId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CartDto>.Success(cartDto));

        var result = await _controller.UpdateItemQuantity(itemId, request, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdateItemQuantity_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        var itemId = Guid.NewGuid();
        var request = new UpdateCartItemRequest(5);

        _mockCartService
            .Setup(x => x.UpdateItemQuantityAsync(_userId, itemId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CartDto>.Failure("Cart item not found"));

        var result = await _controller.UpdateItemQuantity(itemId, request, CancellationToken.None);

        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region RemoveItem Tests

    [Fact]
    public async Task RemoveItem_ShouldReturnOk_WhenItemIsRemoved()
    {
        var itemId = Guid.NewGuid();

        _mockCartService
            .Setup(x => x.RemoveItemAsync(_userId, itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.RemoveItem(itemId, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task RemoveItem_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        var itemId = Guid.NewGuid();

        _mockCartService
            .Setup(x => x.RemoveItemAsync(_userId, itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Cart item not found"));

        var result = await _controller.RemoveItem(itemId, CancellationToken.None);

        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region ClearCart Tests

    [Fact]
    public async Task ClearCart_ShouldReturnOk_WhenCartIsCleared()
    {
        _mockCartService
            .Setup(x => x.ClearCartAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.ClearCart(CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    #endregion

    private static CartDto CreateCartDto()
    {
        return new CartDto(
            Id: Guid.NewGuid(),
            Items: new List<CartItemDto>
            {
                new(
                    Id: Guid.NewGuid(),
                    ProductId: Guid.NewGuid(),
                    ProductName: "Test Product",
                    ProductSlug: "test-product",
                    Quantity: 2,
                    UnitPrice: 50.00m,
                    Total: 100.00m,
                    IsInStock: true
                )
            },
            TotalItems: 2,
            SubTotal: 100.00m,
            IsEmpty: false
        );
    }
}
