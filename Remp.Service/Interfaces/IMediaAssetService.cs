using Remp.Service.DTOs.MediaAsset;

namespace Remp.Service.Interfaces;

public interface IMediaAssetService
{
  Task<IEnumerable<MediaAssetResponse>> UploadMediaAsync(int listingCaseId, UploadMediaRequest request, string userId);
  Task<IEnumerable<MediaAssetResponse>> GetMediaByListingIdAsync(int listingCaseId);
  Task DeleteMediaAsync(int mediaId, string userId);
  Task SetHeroImageAsync(int listingCaseId, int mediaId);
  Task<IEnumerable<MediaAssetResponse>> GetSelectedMediaAsync(int listingCaseId);
  Task UpdateSelectedMediaAsync(int listingCaseId, List<int> mediaIds, string userId);
}
