using ECommerce.Application.Products.DTOs;
using FluentValidation;

namespace ECommerce.Api.Validators;

public class ProductListQueryValidator : AbstractValidator<ProductListQuery>
{
    public ProductListQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than zero");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum price cannot be negative")
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum price cannot be negative")
            .When(x => x.MaxPrice.HasValue);

        RuleFor(x => x.SortBy)
            .MaximumLength(50).WithMessage("Sort by field must not exceed 50 characters")
            .When(x => x.SortBy != null);
    }
}
