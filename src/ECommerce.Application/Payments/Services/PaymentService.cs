using ECommerce.Application.Common.Models;
using ECommerce.Application.Payments.DTOs;
using ECommerce.Application.Payments.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Payments.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;

    public PaymentService(IPaymentRepository paymentRepository, IOrderRepository orderRepository)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
    }

    public async Task<Result<PaymentDto>> CreatePaymentAsync(Guid userId, CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            return Result<PaymentDto>.Failure("Order not found");

        if (order.UserId != userId)
            return Result<PaymentDto>.Failure("Order not found");

        var existingPayments = await _paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        var hasSuccessfulPayment = existingPayments.Any(p => p.IsSuccessful);
        if (hasSuccessfulPayment)
            return Result<PaymentDto>.Failure("A successful payment already exists for this order");

        var amountDifference = Math.Abs(request.Amount - order.TotalAmount);
        if (amountDifference > 0.01m)
            return Result<PaymentDto>.Failure($"Payment amount must match order total. Order total: {order.TotalAmount}, Payment amount: {request.Amount}");

        var payment = Payment.Create(
            request.OrderId,
            request.Method,
            request.Provider,
            request.Amount,
            request.Currency
        );

        var created = await _paymentRepository.AddAsync(payment, cancellationToken);
        order.SetPayment(created.Id);
        await _orderRepository.UpdateAsync(order, cancellationToken);

        return Result<PaymentDto>.Success(MapToDto(created));
    }

    public async Task<Result<PaymentDto>> GetPaymentByIdAsync(Guid userId, Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
            return Result<PaymentDto>.Failure("Payment not found");

        var order = await _orderRepository.GetByIdAsync(payment.OrderId, cancellationToken);
        if (order == null || order.UserId != userId)
            return Result<PaymentDto>.Failure("Payment not found");

        return Result<PaymentDto>.Success(MapToDto(payment));
    }

    public async Task<Result<PaymentDto>> GetPaymentByOrderIdAsync(Guid userId, Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null || order.UserId != userId)
            return Result<PaymentDto>.Failure("Order not found");

        var payments = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        var payment = payments.FirstOrDefault();
        if (payment == null)
            return Result<PaymentDto>.Failure("Payment not found for this order");

        return Result<PaymentDto>.Success(MapToDto(payment));
    }

    public async Task<Result<PaymentDto>> ProcessPaymentAsync(Guid userId, Guid paymentId, string? providerReference = null, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
            return Result<PaymentDto>.Failure("Payment not found");

        var order = await _orderRepository.GetByIdAsync(payment.OrderId, cancellationToken);
        if (order == null || order.UserId != userId)
            return Result<PaymentDto>.Failure("Payment not found");

        try
        {
            var txnId = providerReference ?? $"TXN-{Guid.NewGuid():N}";
            payment.Authorize(txnId);
            payment.MarkAsPaid();
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            return Result<PaymentDto>.Success(MapToDto(payment));
        }
        catch (InvalidOperationException ex)
        {
            return Result<PaymentDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<PaymentDto>> MarkPaymentAsPaidAsync(Guid userId, Guid paymentId, string? providerResponse = null, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
            return Result<PaymentDto>.Failure("Payment not found");

        var order = await _orderRepository.GetByIdAsync(payment.OrderId, cancellationToken);
        if (order == null || order.UserId != userId)
            return Result<PaymentDto>.Failure("Payment not found");

        try
        {
            payment.MarkAsPaid(providerResponse);
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            return Result<PaymentDto>.Success(MapToDto(payment));
        }
        catch (InvalidOperationException ex)
        {
            return Result<PaymentDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<PaymentDto>> MarkPaymentAsFailedAsync(Guid userId, Guid paymentId, string reason, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
            return Result<PaymentDto>.Failure("Payment not found");

        var order = await _orderRepository.GetByIdAsync(payment.OrderId, cancellationToken);
        if (order == null || order.UserId != userId)
            return Result<PaymentDto>.Failure("Payment not found");

        payment.MarkAsFailed(reason);
        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        return Result<PaymentDto>.Success(MapToDto(payment));
    }

    public async Task<Result<PaymentDto>> RefundPaymentAsync(Guid userId, Guid paymentId, decimal? refundAmount = null, string? reason = null, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
            return Result<PaymentDto>.Failure("Payment not found");

        var order = await _orderRepository.GetByIdAsync(payment.OrderId, cancellationToken);
        if (order == null || order.UserId != userId)
            return Result<PaymentDto>.Failure("Payment not found");

        try
        {
            payment.MarkAsRefunded(refundAmount, reason);
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            return Result<PaymentDto>.Success(MapToDto(payment));
        }
        catch (InvalidOperationException ex)
        {
            return Result<PaymentDto>.Failure(ex.Message);
        }
    }

    private static PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto(
            payment.Id,
            payment.OrderId,
            payment.Method.ToString(),
            payment.Provider,
            payment.TransactionId,
            payment.Status.ToString(),
            payment.Amount,
            payment.Currency,
            payment.FailureReason,
            payment.ProcessedAt,
            payment.CreatedAt,
            payment.UpdatedAt
        );
    }
}
