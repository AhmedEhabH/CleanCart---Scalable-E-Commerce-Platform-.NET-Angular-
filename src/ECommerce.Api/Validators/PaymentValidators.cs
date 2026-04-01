using ECommerce.Application.Payments.DTOs;
using FluentValidation;

namespace ECommerce.Api.Validators;

public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");

        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required")
            .MaximumLength(100).WithMessage("Provider must not exceed 100 characters");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code")
            .Matches("^[A-Z]{3}$").WithMessage("Currency must be a valid ISO 4217 currency code (e.g., USD, EUR)");
    }
}

public class ProcessPaymentRequestValidator : AbstractValidator<ProcessPaymentRequest>
{
    public ProcessPaymentRequestValidator()
    {
        RuleFor(x => x.ProviderReference)
            .MaximumLength(100).WithMessage("Provider reference must not exceed 100 characters");

        RuleFor(x => x.ProviderResponse)
            .MaximumLength(2000).WithMessage("Provider response must not exceed 2000 characters");
    }
}

public class RefundPaymentRequestValidator : AbstractValidator<RefundPaymentRequest>
{
    public RefundPaymentRequestValidator()
    {
        RuleFor(x => x.RefundAmount)
            .GreaterThan(0).When(x => x.RefundAmount.HasValue)
            .WithMessage("Refund amount must be greater than zero");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters");
    }
}
