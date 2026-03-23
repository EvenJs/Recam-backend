using Remp.Models.Entities;

namespace Remp.Repository.Interfaces;

public interface IAgentRepository : IBaseRepository<Agent>
{
  Task<Agent?> GetByUserIdAsync(string userId);
  Task<IEnumerable<Agent>> GetByEmailAsync(string email);
  Task<IEnumerable<Agent>> GetByPhotographyCompanyIdAsync(string photographyCompanyId);
}