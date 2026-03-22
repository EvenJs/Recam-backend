using Microsoft.EntityFrameworkCore;
using Remp.DataAccess.Data;
using Remp.Models.Entities;
using Remp.Repository.Interfaces;

namespace Remp.Repository.Repositories;

public class ListingCaseRepository : BaseRepository<ListingCase>, IListingCaseRepository
{
  public ListingCaseRepository(AppDbContext context) : base(context) { }

  public async Task<IEnumerable<ListingCase>> GetByUserIdAsync(string userId)
      => await _dbSet
          .Include(x => x.MediaAssets)
          .Where(x => x.UserId == userId && !x.IsDeleted)
          .ToListAsync();

  public async Task<IEnumerable<ListingCase>> GetByAgentIdAsync(string agentId)
      => await _dbSet
          .Include(x => x.MediaAssets)
          .Where(x => x.AgentListingCases.Any(a => a.AgentId == agentId) && !x.IsDeleted)
          .ToListAsync();

  public async Task<(IEnumerable<ListingCase> Items, int TotalCount)> GetPagedAsync(
      string? userId,
      string? agentId,
      int? statusFilter,
      int page,
      int pageSize)
  {
    var query = _dbSet
        .Include(x => x.MediaAssets)
        .Where(x => !x.IsDeleted);

    if (userId != null)
      query = query.Where(x => x.UserId == userId);

    if (agentId != null)
      query = query.Where(x => x.AgentListingCases.Any(a => a.AgentId == agentId));

    if (statusFilter.HasValue)
      query = query.Where(x => (int)x.ListcaseStatus == statusFilter.Value);

    var totalCount = await query.CountAsync();

    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return (items, totalCount);
  }
}