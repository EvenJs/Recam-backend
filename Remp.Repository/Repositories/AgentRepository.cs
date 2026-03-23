using Microsoft.EntityFrameworkCore;
using Remp.DataAccess.Data;
using Remp.Models.Entities;
using Remp.Repository.Interfaces;


namespace Remp.Repository.Repositories;

public class AgentRepository : BaseRepository<Agent>, IAgentRepository
{
  public AgentRepository(AppDbContext context) : base(context) { }

  public async Task<Agent?> GetByUserIdAsync(string userId)
    => await _dbSet.FirstOrDefaultAsync(x => x.Id == userId);

  public async Task<IEnumerable<Agent>> GetByEmailAsync(string email)
      => await _dbSet
          .Where(x => x.Email != null && x.Email.Contains(email))
          .ToListAsync();
  public async Task<IEnumerable<Agent>> GetByPhotographyCompanyIdAsync(string photographyCompanyId)
    => await _dbSet
      .Where(x => x.AgentPhotographyCompanies.Any(apc => apc.PhotographyCompanyId == photographyCompanyId))
      .ToListAsync();
}