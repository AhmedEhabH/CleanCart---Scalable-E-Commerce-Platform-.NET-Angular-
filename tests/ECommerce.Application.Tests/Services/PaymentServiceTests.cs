using ECommerce.Application.Cart.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Orders.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Interfaces;
using ECommerce.Domain.ValueObjects;
using ECommerce.Application.Payments.DTOs;
using ECommerce.Application.Payments.Services;
using FluentAssertions;
using Moq;
using Xunit;
using VOAddress = ECommerce.Domain.ValueObjects.Address;

namespace ECommerce.Application.Tests.Services;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _sut = new PaymentService(_paymentRepositoryMock.Object, _orderRepositoryMock.Object);
    }

    #region CreatePaymentAsync Tests

    [Fact]
    public async Task CreatePaymentAsync_ShouldReturnFailure_WhenOrderNotFound()
    {
        var userId = Guid.NewGuid();
        var request = new CreatePaymentRequest(Guid.NewGuid(), PaymentMethod.CreditCard, "stripe", 100m, "USD");

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(request.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await _sut.CreatePaymentAsync(userId, request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Order not found");
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldReturnFailure_WhenOrderBelongsToDifferentUser()
    {
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var order = CreateOrder(differentUserId);
        var request = new CreatePaymentRequest(order.Id, PaymentMethod.CreditCard, "stripe", 100m, "USD");

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _sut.CreatePaymentAsync(userId, request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Order not found");
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldReturnFailure_WhenAmountDoesNotMatchOrderTotal()
    {
        var userId = Guid.NewGuid();
        var order = CreateOrder(userId, 100m);
        var request = new CreatePaymentRequest(order.Id, PaymentMethod.CreditCard, "stripe", 50m, "USD");

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _paymentRepositoryMock.Setup(r => r.GetByOrderIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Payment>());

        var result = await _sut.CreatePaymentAsync(userId, request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("must match order total");
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldReturnFailure_WhenSuccessfulPaymentExists()
    {
        var userId = Guid.NewGuid();
        var order = CreateOrder(userId, 100m);
        var request = new CreatePaymentRequest(order.Id, PaymentMethod.CreditCard, "stripe", 100m, "USD");
        var existingPayment = CreatePayment(order.Id, PaymentStatus.Paid);

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _paymentRepositoryMock.Setup(r => r.GetByOrderIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Payment> { existingPayment });

        var result = await _sut.CreatePaymentAsync(userId, request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("successful payment already exists");
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldCallAddAndUpdate_WhenValidRequest()
    {
        var userId = Guid.NewGuid();
        var order = CreateOrder(userId, 100m);
        var request = new CreatePaymentRequest(order.Id, PaymentMethod.CreditCard, "stripe", order.TotalAmount, "USD");

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _paymentRepositoryMock.Setup(r => r.GetByOrderIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Payment>());
        _paymentRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment p, CancellationToken _) => p);
        _orderRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.CreatePaymentAsync(userId, request);

        _paymentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetPaymentByIdAsync Tests

    [Fact]
    public async Task GetPaymentByIdAsync_ShouldReturnFailure_WhenPaymentNotFound()
    {
        var userId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();

        _paymentRepositoryMock.Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var result = await _sut.GetPaymentByIdAsync(userId, paymentId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Payment not found");
    }

    [Fact]
    public async Task GetPaymentByIdAsync_ShouldReturnFailure_WhenOrderBelongsToDifferentUser()
    {
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var order = CreateOrder(differentUserId);
        var payment = CreatePayment(order.Id);

        _paymentRepositoryMock.Setup(r => r.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _sut.GetPaymentByIdAsync(userId, payment.Id);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetPaymentByIdAsync_ShouldReturnPayment_WhenUserOwnsOrder()
    {
        var userId = Guid.NewGuid();
        var order = CreateOrder(userId);
        var payment = CreatePayment(order.Id);

        _paymentRepositoryMock.Setup(r => r.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _sut.GetPaymentByIdAsync(userId, payment.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    #endregion

    #region ProcessPaymentAsync Tests

    [Fact]
    public async Task ProcessPaymentAsync_ShouldReturnFailure_WhenPaymentNotFound()
    {
        var userId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();

        _paymentRepositoryMock.Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var result = await _sut.ProcessPaymentAsync(userId, paymentId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Payment not found");
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldProcessPayment_WhenValid()
    {
        var userId = Guid.NewGuid();
        var order = CreateOrder(userId);
        var payment = CreatePayment(order.Id, PaymentStatus.Pending);

        _paymentRepositoryMock.Setup(r => r.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _paymentRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.ProcessPaymentAsync(userId, payment.Id, "txn_123");

        result.IsSuccess.Should().BeTrue();
        _paymentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region RefundPaymentAsync Tests

    [Fact]
    public async Task RefundPaymentAsync_ShouldReturnFailure_WhenPaymentNotPaid()
    {
        var userId = Guid.NewGuid();
        var order = CreateOrder(userId);
        var payment = CreatePayment(order.Id, PaymentStatus.Pending);

        _paymentRepositoryMock.Setup(r => r.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _sut.RefundPaymentAsync(userId, payment.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Only paid payments can be refunded");
    }

    [Fact]
    public async Task RefundPaymentAsync_ShouldRefundPayment_WhenValid()
    {
        var userId = Guid.NewGuid();
        var order = CreateOrder(userId);
        var payment = CreatePayment(order.Id, PaymentStatus.Paid);

        _paymentRepositoryMock.Setup(r => r.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _paymentRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.RefundPaymentAsync(userId, payment.Id, 50m, "Customer request");

        result.IsSuccess.Should().BeTrue();
        _paymentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static Order CreateOrder(Guid userId, decimal totalAmount = 100m)
    {
        var address = VOAddress.Create("123 Main St", "New York", "NY", "10001", "USA");
        var order = Order.Create(
            userId,
            totalAmount,
            10m,
            0m,
            0m,
            address,
            null,
            null
        );
        return order;
    }

    private static Payment CreatePayment(Guid orderId, PaymentStatus status = PaymentStatus.Pending)
    {
        var payment = Payment.Create(orderId, PaymentMethod.CreditCard, "stripe", 100m, "USD");
        if (status == PaymentStatus.Paid)
        {
            payment.Authorize("txn_123");
            payment.MarkAsPaid();
        }
        return payment;
    }

    #endregion
}
