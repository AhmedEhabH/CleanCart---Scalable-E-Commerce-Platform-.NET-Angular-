using ECommerce.Application.Products.DTOs;
using FluentValidation;

namespace ECommerce.Api.Validators;

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters")
            .When(x => x.Name != null);

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.CompareAtPrice)
            .GreaterThan(0).WithMessage("Compare at price must be greater than zero")
            .When(x => x.CompareAtPrice.HasValue);

        RuleFor(x => x.LowStockThreshold)
            .GreaterThanOrEqualTo(0).WithMessage("Low stock threshold cannot be negative")
            .When(x => x.LowStockThreshold.HasValue);
    }
}
