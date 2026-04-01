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

    public async Task<Result<PaymentDto>> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            return Result<PaymentDto>.Failure("Order not found");

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

    public async Task<Result<PaymentDto>> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
            return Result<PaymentDto>.Failure("Payment not found");

        return Result<PaymentDto>.Success(MapToDto(payment));
    }

    public async Task<Result<PaymentDto>> GetPaymentByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        var payment = payments.FirstOrDefault();
        if (payment == null)
            return Result<PaymentDto>.Failure("Payment not found for this order");

        return Result<PaymentDto>.Success(MapToDto(payment));
    }

    public async Task<Result<PaymentDto>> ProcessPaymentAsync(Guid paymentId, string? providerReference = null, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
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

    public async Task<Result<PaymentDto>> MarkPaymentAsPaidAsync(Guid paymentId, string? providerResponse = null, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
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

    public async Task<Result<PaymentDto>> MarkPaymentAsFailedAsync(Guid paymentId, string reason, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
            return Result<PaymentDto>.Failure("Payment not found");

        payment.MarkAsFailed(reason);
        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        return Result<PaymentDto>.Success(MapToDto(payment));
    }

    public async Task<Result<PaymentDto>> RefundPaymentAsync(Guid paymentId, decimal? refundAmount = null, string? reason = null, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
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
