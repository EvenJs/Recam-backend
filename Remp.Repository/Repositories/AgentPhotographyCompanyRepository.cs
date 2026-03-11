using Microsoft.EntityFrameworkCore;
using Remp.DataAccess.Data;
using Remp.Models.Entities;
using Remp.Repository.Interfaces;

namespace Remp.Repository.Repositories;

public class AgentPhotographyCompanyRepository : BaseRepository<AgentPhotographyCompany>, IAgentPhotographyCompanyRepository
{
  public AgentPhotographyCompanyRepository(AppDbContext context) : base(context) { }

  public async Task<bool> ExistsAsync(string agentId, string photographyCompanyId)
    => await _dbSet
      .AnyAsync(x => x.AgentId == agentId && x.PhotographyCompanyId == photographyCompanyId);

  public async Task<IEnumerable<AgentPhotographyCompany>> GetByPhotographyCompanyIdAsync(string photographyCompanyId)
    => await _dbSet
      .Where(x => x.PhotographyCompanyId == photographyCompanyId)
      .ToListAsync();
}



