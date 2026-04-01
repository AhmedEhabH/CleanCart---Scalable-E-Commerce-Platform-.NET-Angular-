using ECommerce.Application.Common.Models;
using ECommerce.Application.Payments.DTOs;

namespace ECommerce.Application.Payments.Interfaces;

public interface IPaymentService
{
    Task<Result<PaymentDto>> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> GetPaymentByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> ProcessPaymentAsync(Guid paymentId, string? providerReference = null, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> MarkPaymentAsPaidAsync(Guid paymentId, string? providerResponse = null, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> MarkPaymentAsFailedAsync(Guid paymentId, string reason, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> RefundPaymentAsync(Guid paymentId, decimal? refundAmount = null, string? reason = null, CancellationToken cancellationToken = default);
}
