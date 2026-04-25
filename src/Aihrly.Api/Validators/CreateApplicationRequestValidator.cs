using Aihrly.Api.DTOs.Requests;
using FluentValidation;

namespace Aihrly.Api.Validators;

public class CreateApplicationRequestValidator : AbstractValidator<CreateApplicationRequest>
{
    public CreateApplicationRequestValidator()
    {
        RuleFor(x => x.CandidateName)
            .NotEmpty().WithMessage("Candidate name is required.")
            .MaximumLength(200).WithMessage("Candidate name cannot exceed 200 characters.");

        RuleFor(x => x.CandidateEmail)
            .NotEmpty().WithMessage("Candidate email is required.")
            .EmailAddress().WithMessage("Candidate email must be a valid email address.")
            .MaximumLength(200).WithMessage("Candidate email cannot exceed 200 characters.");
    }
}
