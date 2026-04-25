using Aihrly.Api.DTOs.Requests;
using FluentValidation;

namespace Aihrly.Api.Validators;

public class ScoreRequestValidator : AbstractValidator<ScoreRequest>
{
    public ScoreRequestValidator()
    {
        RuleFor(x => x.Score)
            .InclusiveBetween(1, 5).WithMessage("Score must be between 1 and 5.");
    }
}
