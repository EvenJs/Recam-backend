using Remp.Models.Enums;

namespace Remp.Service.DTOs.MediaAsset;

public class MediaAssetResponse
{
  public int Id { get; set; }
  public MediaType MediaType { get; set; }
  public string MediaUrl { get; set; } = string.Empty;
  public bool IsSelect { get; set; }
  public bool IsHero { get; set; }
  public DateTime UploadedAt { get; set; }
  public int ListingCaseId { get; set; }
  public string UserId { get; set; } = string.Empty; 
}
