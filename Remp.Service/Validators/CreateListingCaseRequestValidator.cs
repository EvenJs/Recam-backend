using FluentValidation;
using Remp.Service.DTOs.ListingCase;

namespace Remp.Service.Validators;

public class CreateListingCaseRequestValidator : AbstractValidator<CreateListingCaseRequest>
{
  public CreateListingCaseRequestValidator()
  {
    RuleFor(x => x.Title)
      .NotEmpty().WithMessage("Title is required.")
      .MaximumLength(255).WithMessage("Title must not exceed 255 characters.");

    RuleFor(x => x.Street)
      .NotEmpty().WithMessage("Street is required.");

    RuleFor(x => x.City)
      .NotEmpty().WithMessage("City is required.");

    RuleFor(x => x.State)
      .NotEmpty().WithMessage("State is required.");

    RuleFor(x => x.Postcode)
      .GreaterThan(0).WithMessage("Postcode is required.");

    RuleFor(x => x.Bedrooms)
      .GreaterThanOrEqualTo(0).WithMessage("Bedrooms must be 0 or more.");

    RuleFor(x => x.Bathrooms)
      .GreaterThanOrEqualTo(0).WithMessage("Bathrooms must be 0 or more.");

    RuleFor(x => x.Garages)
      .GreaterThanOrEqualTo(0).WithMessage("Garages must be 0 or more.");

    RuleFor(x => x.PropertyType)
      .IsInEnum().WithMessage("Invalid property type.");

    RuleFor(x => x.SaleCategory)
      .IsInEnum().WithMessage("Invalid sale category.");

    RuleFor(x => x.Price)
      .GreaterThanOrEqualTo(0).WithMessage("Price must be 0 or more.")
      .When(x => x.Price.HasValue);

    RuleFor(x => x.FloorArea)
      .GreaterThan(0).WithMessage("Floor area must be greater than 0.")
      .When(x => x.FloorArea.HasValue);
  }
}
