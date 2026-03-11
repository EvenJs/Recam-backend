using Remp.Models.Entities;

namespace Remp.Repository.Interfaces;

public interface IAgentPhotographyCompanyRepository : IBaseRepository<AgentPhotographyCompany>
{
  Task<bool> ExistsAsync(string agentId, string photographyCompanyId);
  Task<IEnumerable<AgentPhotographyCompany>> GetByPhotographyCompanyIdAsync(string photographyCompanyId);
}