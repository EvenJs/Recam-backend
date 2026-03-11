using Remp.Models.Entities;

namespace Remp.Repository.Interfaces;

public interface IMediaAssetRepository : IBaseRepository<MediaAsset>
{
  Task<IEnumerable<MediaAsset>> GetByListingIdAsync(int listingCaseId);
  Task<MediaAsset?> GetHeroByListingIdAsync(int listingCaseId);
  Task<IEnumerable<MediaAsset>> GetSelectedByListingIdAsync(int listingCaseId);
  Task<int> CountSelectedByListingIdAsync(int listingCaseId);
}