using ECommerce.Application.Common.Models;
using ECommerce.Application.Payments.DTOs;

namespace ECommerce.Application.Payments.Interfaces;

public interface IPaymentService
{
    Task<Result<PaymentDto>> CreatePaymentAsync(Guid userId, CreatePaymentRequest request, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> GetPaymentByIdAsync(Guid userId, Guid paymentId, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> GetPaymentByOrderIdAsync(Guid userId, Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> ProcessPaymentAsync(Guid userId, Guid paymentId, string? providerReference = null, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> MarkPaymentAsPaidAsync(Guid userId, Guid paymentId, string? providerResponse = null, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> MarkPaymentAsFailedAsync(Guid userId, Guid paymentId, string reason, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> RefundPaymentAsync(Guid userId, Guid paymentId, decimal? refundAmount = null, string? reason = null, CancellationToken cancellationToken = default);
}
