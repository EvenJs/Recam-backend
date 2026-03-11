using Remp.Models.Entities;

namespace Remp.Repository.Interfaces;

public interface IAgentListingCaseRepository : IBaseRepository<AgentListingCase>
{
  Task<IEnumerable<AgentListingCase>> GetByListingIdAsync(int listingCaseId);
  Task<IEnumerable<AgentListingCase>> GetByAgentIdAsync(string agentId);
  Task<bool> ExistsAsync(string agentId, int listingCaseId);
}