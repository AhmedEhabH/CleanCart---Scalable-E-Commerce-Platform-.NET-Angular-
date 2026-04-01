using ECommerce.Api.Controllers;
using ECommerce.Api.Models;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Payments.DTOs;
using ECommerce.Application.Payments.Interfaces;
using ECommerce.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

namespace ECommerce.Api.Tests.Controllers;

public class PaymentsControllerTests
{
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly PaymentsController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public PaymentsControllerTests()
    {
        _mockPaymentService = new Mock<IPaymentService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockCurrentUserService.Setup(x => x.UserId).Returns(_userId);
        _controller = new PaymentsController(_mockPaymentService.Object, _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task InitiatePayment_ShouldReturnCreated_WhenPaymentIsSuccessful()
    {
        var request = new CreatePaymentRequest(
            Guid.NewGuid(),
            PaymentMethod.CreditCard,
            "stripe",
            100.00m,
            "USD");

        var paymentDto = CreatePaymentDto(request.OrderId);

        _mockPaymentService
            .Setup(x => x.CreatePaymentAsync(_userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentDto>.Success(paymentDto));

        var result = await _controller.InitiatePayment(request, CancellationToken.None);

        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().NotBeNull();
        _mockPaymentService.Verify(x => x.CreatePaymentAsync(_userId, request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InitiatePayment_ShouldReturnBadRequest_WhenServiceFails()
    {
        var request = new CreatePaymentRequest(
            Guid.NewGuid(),
            PaymentMethod.CreditCard,
            "stripe",
            100.00m,
            "USD");

        _mockPaymentService
            .Setup(x => x.CreatePaymentAsync(_userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentDto>.Failure("Order not found"));

        var result = await _controller.InitiatePayment(request, CancellationToken.None);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task InitiatePayment_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(x => x.UserId).Returns((Guid?)null);
        var controller = new PaymentsController(_mockPaymentService.Object, mockCurrentUserService.Object);

        var request = new CreatePaymentRequest(
            Guid.NewGuid(),
            PaymentMethod.CreditCard,
            "stripe",
            100.00m,
            "USD");

        var result = await controller.InitiatePayment(request, CancellationToken.None);

        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task GetPayment_ShouldReturnOk_WhenPaymentExists()
    {
        var paymentId = Guid.NewGuid();
        var paymentDto = CreatePaymentDto(paymentId);

        _mockPaymentService
            .Setup(x => x.GetPaymentByIdAsync(_userId, paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentDto>.Success(paymentDto));

        var result = await _controller.GetPayment(paymentId, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        _mockPaymentService.Verify(x => x.GetPaymentByIdAsync(_userId, paymentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPayment_ShouldReturnNotFound_WhenPaymentDoesNotExist()
    {
        var paymentId = Guid.NewGuid();

        _mockPaymentService
            .Setup(x => x.GetPaymentByIdAsync(_userId, paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentDto>.Failure("Payment not found"));

        var result = await _controller.GetPayment(paymentId, CancellationToken.None);

        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetPaymentByOrder_ShouldReturnOk_WhenPaymentExists()
    {
        var orderId = Guid.NewGuid();
        var paymentDto = CreatePaymentDto(orderId);

        _mockPaymentService
            .Setup(x => x.GetPaymentByOrderIdAsync(_userId, orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentDto>.Success(paymentDto));

        var result = await _controller.GetPaymentByOrder(orderId, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        _mockPaymentService.Verify(x => x.GetPaymentByOrderIdAsync(_userId, orderId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_ShouldReturnOk_WhenPaymentIsProcessed()
    {
        var paymentId = Guid.NewGuid();
        var providerReference = "ch_1234567890";
        var paymentDto = CreatePaymentDto(paymentId, providerReference, "Paid");

        _mockPaymentService
            .Setup(x => x.ProcessPaymentAsync(_userId, paymentId, providerReference, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentDto>.Success(paymentDto));

        var result = await _controller.ProcessPayment(paymentId, new ProcessPaymentRequest(paymentId, providerReference, null), CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        _mockPaymentService.Verify(x => x.ProcessPaymentAsync(_userId, paymentId, providerReference, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_ShouldReturnBadRequest_WhenServiceFails()
    {
        var paymentId = Guid.NewGuid();

        _mockPaymentService
            .Setup(x => x.ProcessPaymentAsync(_userId, paymentId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentDto>.Failure("Payment not found"));

        var result = await _controller.ProcessPayment(paymentId, new ProcessPaymentRequest(paymentId, null, null), CancellationToken.None);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task RefundPayment_ShouldReturnOk_WhenPaymentIsRefunded()
    {
        var paymentId = Guid.NewGuid();
        var paymentDto = CreatePaymentDto(paymentId, "ch_1234567890", "Refunded");

        _mockPaymentService
            .Setup(x => x.RefundPaymentAsync(_userId, paymentId, 50.00m, "Customer requested refund", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentDto>.Success(paymentDto));

        var result = await _controller.RefundPayment(paymentId, new RefundPaymentRequest(paymentId, 50.00m, "Customer requested refund"), CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        _mockPaymentService.Verify(x => x.RefundPaymentAsync(_userId, paymentId, 50.00m, "Customer requested refund", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefundPayment_ShouldReturnBadRequest_WhenPaymentAlreadyRefunded()
    {
        var paymentId = Guid.NewGuid();

        _mockPaymentService
            .Setup(x => x.RefundPaymentAsync(_userId, paymentId, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentDto>.Failure("Only paid payments can be refunded"));

        var result = await _controller.RefundPayment(paymentId, new RefundPaymentRequest(paymentId, null, null), CancellationToken.None);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    private static PaymentDto CreatePaymentDto(Guid orderId, string? transactionId = null, string status = "Pending")
    {
        return new PaymentDto(
            Guid.NewGuid(),
            orderId,
            PaymentMethod.CreditCard.ToString(),
            "stripe",
            transactionId,
            status,
            100.00m,
            "USD",
            null,
            status == "Paid" ? DateTime.UtcNow : null,
            DateTime.UtcNow,
            DateTime.UtcNow);
    }
}
