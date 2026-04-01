using System.Collections.Immutable;

namespace ECommerce.Application.Common.Interfaces;

public interface IValidationService
{
    Task<ValidationResult> ValidateAsync<TValidator, TRequest>(TRequest request) 
        where TValidator : class
        where TRequest : class;
}

public class ValidationResult
{
    public bool IsValid { get; }
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationResult(bool isValid, IReadOnlyDictionary<string, string[]>? errors = null)
    {
        IsValid = isValid;
        Errors = errors ?? ImmutableDictionary<string, string[]>.Empty;
    }

    public static ValidationResult Success() => new(true);
    public static ValidationResult Failure(IReadOnlyDictionary<string, string[]> errors) => new(false, errors);
}
