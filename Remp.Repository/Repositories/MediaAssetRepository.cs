using Microsoft.EntityFrameworkCore;
using Remp.DataAccess.Data;
using Remp.Models.Entities;
using Remp.Repository.Interfaces;

namespace Remp.Repository.Repositories;

public class MediaAssetRepository : BaseRepository<MediaAsset>, IMediaAssetRepository
{
  public MediaAssetRepository(AppDbContext context) : base(context){ }

  public async Task<IEnumerable<MediaAsset>> GetByListingIdAsync(int listingCaseId)
    => await _dbSet
      .Where(x => x.ListingCaseId == listingCaseId && !x.IsDeleted)
      .ToListAsync();

  public async Task<MediaAsset?> GetHeroByListingIdAsync(int listingCaseId)
    => await _dbSet
      .FirstOrDefaultAsync(x => x.ListingCaseId == listingCaseId && x.IsHero && !x.IsDeleted);

  public async Task<IEnumerable<MediaAsset>> GetSelectedByListingIdAsync(int listingCaseId)
    => await _dbSet
      .Where(x => x.ListingCaseId == listingCaseId && x.IsSelect && !x.IsDeleted)
      .ToListAsync();

  public async Task<int> CountSelectedByListingIdAsync(int listingCaseId)
    => await _dbSet
      .CountAsync(x => x.ListingCaseId == listingCaseId && x.IsDeleted && !x.IsDeleted);
}