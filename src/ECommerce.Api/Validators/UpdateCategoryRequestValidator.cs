using ECommerce.Application.Categories.DTOs;
using FluentValidation;

namespace ECommerce.Api.Validators;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters")
            .When(x => x.Name != null);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.IconUrl)
            .MaximumLength(500).WithMessage("Icon URL must not exceed 500 characters")
            .When(x => x.IconUrl != null);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order cannot be negative")
            .When(x => x.DisplayOrder.HasValue);
    }
}
