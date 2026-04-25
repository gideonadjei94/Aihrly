using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace Aihrly.Api.Common;

public static class ValidationExtensions
{
    // Converts validation failures into the same problem+json shape used everywhere else
    public static ValidationProblemDetails ToValidationProblemDetails(this ValidationResult result)
    {
        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return new ValidationProblemDetails(errors)
        {
            Title = "Validation failed",
            Status = 400
        };
    }
}
