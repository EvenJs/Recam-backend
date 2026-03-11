using Remp.Models.Enums;

namespace Remp.Models.Entities;

public class MediaAsset
{
  public int Id { get; set; }
  public MediaType MediaType { get; set; }
  public string MediaUrl { get; set; } = string.Empty;
  public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
  public bool IsSelect { get; set; } = false;
  public bool IsHero { get; set; } = false;
  public bool IsDeleted { get; set; } = false;

  // Foreign Keys
  public int ListingCaseId { get; set; }
  public string UserId { get; set; } = string.Empty;

  // Navigation properties
  public ListingCase ListingCase { get; set; } = null!;
  public ApplicationUser UploadedBy { get; set; } = null!;
}