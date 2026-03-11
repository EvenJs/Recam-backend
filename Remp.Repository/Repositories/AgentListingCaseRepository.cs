using Microsoft.EntityFrameworkCore;
using Remp.DataAccess.Data;
using Remp.Models.Entities;
using Remp.Repository.Interfaces;

namespace Remp.Repository.Repositories;

public class AgentListingCaseRepository : BaseRepository<AgentListingCase>, IAgentListingCaseRepository
{
  public AgentListingCaseRepository(AppDbContext context) : base(context) { }

  public async Task<IEnumerable<AgentListingCase>> GetByListingIdAsync(int listingCaseId)
    => await _dbSet
      .Where(x => x.ListingCaseId == listingCaseId)
      .ToListAsync();

  public async Task<IEnumerable<AgentListingCase>> GetByAgentIdAsync(string agentId)
    => await _dbSet
      .Where(x => x.AgentId ==agentId)
      .ToListAsync();
  
  public async Task<bool> ExistsAsync(string agentId, int listingCaseId)
    => await _dbSet
      .AnyAsync(x => x.AgentId ==agentId && x.ListingCaseId == listingCaseId);
}