using FluentValidation;
using Remp.Service.DTOs.Auth;

namespace Remp.Service.Validators;

public class RegisterAgentRequestValidator : AbstractValidator<RegisterAgentRequest>
{
  public RegisterAgentRequestValidator()
  {
    RuleFor(x => x.Email)
      .NotEmpty().WithMessage("Email is required.")
      .EmailAddress().WithMessage("A valid email is required.");

    RuleFor(x => x.AgentFirstName)
      .NotEmpty().WithMessage("Fist name is required.")
      .MaximumLength(100).WithMessage("First name must nor exceed 100 characters.");

    RuleFor(x => x.AgentLastName)
      .NotEmpty().WithMessage("Last name is required.")
      .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

    RuleFor(x => x.CompanyName)
      .MaximumLength(255).WithMessage("Company name must not exceed 255 characters.")
      .When(x => x.CompanyName != null);
  }

}
