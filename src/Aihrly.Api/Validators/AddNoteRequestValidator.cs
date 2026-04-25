using Aihrly.Api.Domain;
using Aihrly.Api.DTOs.Requests;
using Aihrly.Api.Enums;
using FluentValidation;

namespace Aihrly.Api.Validators;

public class AddNoteRequestValidator : AbstractValidator<AddNoteRequest>
{
    public AddNoteRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Note type is required.")
            .Must(value => EnumParser.TryParse<NoteType>(value, out _))
            .WithMessage($"Type must be one of: {string.Join(", ", Enum.GetNames<NoteType>())}.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");
    }
}
