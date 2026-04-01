using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.Api.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    private readonly IEnumerable<IValidator> _validators;

    public ValidationFilter(IEnumerable<IValidator> validators)
    {
        _validators = validators;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ActionArguments.Any())
        {
            await next();
            return;
        }

        var validationErrors = new Dictionary<string, string[]>();

        foreach (var argument in context.ActionArguments)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(argument.Value!.GetType());
            var validator = _validators.FirstOrDefault(v => validatorType.IsInstanceOfType(v));

            if (validator != null)
            {
                var validateMethod = validatorType.GetMethod("ValidateAsync", new[] { argument.Value.GetType() });
                var validationResult = await (Task<ValidationResult>)validateMethod!.Invoke(validator, new[] { argument.Value })!;
                
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        var propertyName = ToCamelCase(error.PropertyName);
                        if (validationErrors.ContainsKey(propertyName))
                        {
                            validationErrors[propertyName] = validationErrors[propertyName]
                                .Concat(new[] { error.ErrorMessage }).ToArray();
                        }
                        else
                        {
                            validationErrors[propertyName] = new[] { error.ErrorMessage };
                        }
                    }
                }
            }
        }

        if (validationErrors.Any())
        {
            context.Result = new BadRequestObjectResult(new
            {
                message = "Validation failed",
                errors = validationErrors
            });
            return;
        }

        await next();
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;
        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}
