using Aihrly.Api.Domain;
using Aihrly.Api.DTOs.Requests;
using Aihrly.Api.Enums;
using FluentValidation;

namespace Aihrly.Api.Validators;

public class MoveStageRequestValidator : AbstractValidator<MoveStageRequest>
{
    public MoveStageRequestValidator()
    {
        RuleFor(x => x.Stage)
            .NotEmpty().WithMessage("Stage is required.")
            .Must(value => EnumParser.TryParse<ApplicationStage>(value, out _))
            .WithMessage($"Stage must be one of: {string.Join(", ", Enum.GetNames<ApplicationStage>())}.");
    }
}
