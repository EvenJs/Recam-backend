using FluentValidation;
using Remp.Service.DTOs.Auth;

namespace Remp.Service.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
  public LoginRequestValidator()
  {
    RuleFor(x => x.Email)
      .NotEmpty().WithMessage("Email is required.")
      .EmailAddress().WithMessage("A valid email is required.");

    RuleFor(x => x.Password)
      .NotEmpty().WithMessage("Password is required.");
  }
}
