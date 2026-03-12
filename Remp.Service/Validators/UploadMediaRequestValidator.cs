using FluentValidation;
using Remp.Models.Enums;
using Remp.Service.DTOs.MediaAsset;

namespace Remp.Service.Validators;

public class UploadMediaRequestValidator : AbstractValidator<UploadMediaRequest>
{
  private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png"];
  private static readonly string[] AllowedVideoExtensions = [".mp4", ".mov"];
  private static readonly string[] AllowedFloorPlanExtensions = [".pdf"];
  private static readonly string[] AllowedVrExtensions = [".gltf"];

  public UploadMediaRequestValidator()
  {
    RuleFor(x => x.Files)
      .NotEmpty().WithMessage("At least one file is required.");

    RuleFor(x => x.MediaType)
      .IsInEnum().WithMessage("Invalid media type.");

    RuleFor(x => x.Files)
      .Must((request, files) => files.Count == 1)
      .When(x => x.MediaType != MediaType.Picture)
      .WithMessage("Only one file is allowed for Video, Floor Plan, and VR Tour uploads.");

    RuleForEach(x => x.Files).ChildRules(file =>
    {
      file.RuleFor(x => x.Length)
        .GreaterThan(0).WithMessage("File must not be empty.");
    });

    RuleFor(x => x.Files)
      .Must((request, files) =>
      {
        var allowedExtensions = request.MediaType switch
        {
          MediaType.Picture => AllowedImageExtensions,
          MediaType.Video => AllowedVideoExtensions,
          MediaType.FloorPlan => AllowedFloorPlanExtensions,
          MediaType.VRTour => AllowedVrExtensions,
          _ => Array.Empty<string>()
        };

        return files.All(f =>
          allowedExtensions.Contains(
            Path.GetExtension(f.FileName).ToLowerInvariant()));
      })
      .WithMessage("One or more files have an invalid extension for the selected media type.");
  }

}
