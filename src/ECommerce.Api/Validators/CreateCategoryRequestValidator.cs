using ECommerce.Application.Categories.DTOs;
using FluentValidation;

namespace ECommerce.Api.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required")
            .MaximumLength(100).WithMessage("Slug must not exceed 100 characters")
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage("Slug must be URL-friendly (lowercase letters, numbers, and hyphens only)");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.IconUrl)
            .MaximumLength(500).WithMessage("Icon URL must not exceed 500 characters")
            .When(x => x.IconUrl != null);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order cannot be negative");
    }
}
