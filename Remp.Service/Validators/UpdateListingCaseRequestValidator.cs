using FluentValidation;
using Remp.Service.DTOs.ListingCase;

namespace Remp.Service.Validators;

public class UpdateListingCaseRequestValidator : AbstractValidator<UpdateListingCaseRequest>
{
  public UpdateListingCaseRequestValidator()
  {
    RuleFor(x => x.Title)
      .MaximumLength(255).WithMessage("Title must not exceed 255 characters.")
      .When(x => x.Title != null);

    RuleFor(x => x.Postcode)
      .GreaterThan(0).WithMessage("Postcode must be greater than 0.")
      .When(x => x.Postcode.HasValue);

    RuleFor(x => x.Bedrooms)
      .GreaterThanOrEqualTo(0).WithMessage("Bedrooms must be 0 or more.")
      .When(x => x.Bedrooms.HasValue);

    RuleFor(x => x.Bathrooms)
      .GreaterThanOrEqualTo(0).WithMessage("Bathrooms must be 0 or more.")
      .When(x => x.Bathrooms.HasValue);

    RuleFor(x => x.Garages)
      .GreaterThanOrEqualTo(0).WithMessage("Garages must be 0 or more.")
      .When(x => x.Garages.HasValue);

    RuleFor(x => x.PropertyType)
      .IsInEnum().WithMessage("Invalid property type.")
      .When(x => x.PropertyType.HasValue);

    RuleFor(x => x.SaleCategory)
      .IsInEnum().WithMessage("Invalid sale category.")
      .When(x => x.SaleCategory.HasValue);

    RuleFor(x => x.Price)
      .GreaterThanOrEqualTo(0).WithMessage("Price must be 0 or more.")
      .When(x => x.Price.HasValue);

    RuleFor(x => x.FloorArea)
      .GreaterThan(0).WithMessage("Floor area must be greater than 0.")
      .When(x => x.FloorArea.HasValue);
}
}
