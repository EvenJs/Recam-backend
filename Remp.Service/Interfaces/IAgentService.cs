using Remp.Service.DTOs.Agent;
using Remp.Service.DTOs.Auth;

namespace Remp.Service.Interfaces;

public interface IAgentService
{
  Task<AgentResponse> CreateAgentAsync(RegisterAgentRequest request, string photographyCompanyId);
  Task<IEnumerable<AgentResponse>> GetAgentsByCompanyAsync(string photographyCompanyId);
  Task<AgentResponse> GetAgentByEmailAsync(string email);
  Task LinkAgentToCompanyAsync(string agentId, string photographyCompanyId);
}
