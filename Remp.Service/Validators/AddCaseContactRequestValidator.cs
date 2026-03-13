using FluentValidation;
using Remp.Service.DTOs.CaseContact;

namespace Remp.Service.Validators;

public class AddCaseContactRequestValidator : AbstractValidator<AddCaseContactRequest>
{
  public AddCaseContactRequestValidator()
  {
    RuleFor(x => x.FirstName)
      .NotEmpty().WithMessage("First name is required.")
      .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

    RuleFor(x => x.LastName)
      .NotEmpty().WithMessage("Last name is required.")
      .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

    RuleFor(x => x.Email)
      .NotEmpty().WithMessage("Email is required.")
      .EmailAddress().WithMessage("A valid email address is required.");

    RuleFor(x => x.PhoneNumber)
      .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
      .When(x => x.PhoneNumber != null);

    RuleFor(x => x.CompanyName)
      .MaximumLength(255).WithMessage("Company name must not exceed 255 characters.")
      .When(x => x.CompanyName != null);
  }
}
