using Microsoft.EntityFrameworkCore;
using Remp.DataAccess.Data;
using Remp.Models.Entities;
using Remp.Repository.Interfaces;

namespace Remp.Repository.Repositories;

public class CaseContactRepository : BaseRepository<CaseContact>, ICaseContactRepository
{
  public CaseContactRepository(AppDbContext context) : base(context) { }

  public async Task<IEnumerable<CaseContact>> GetByListingIdAsync(int listingCaseId)
    => await _dbSet
      .Where(x => x.ListingCaseId == listingCaseId)
      .ToListAsync();
}