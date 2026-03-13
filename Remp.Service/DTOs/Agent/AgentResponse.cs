
namespace Remp.Service.DTOs.Agent;

public class AgentResponse
{
  public string Id { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string AgentFirstName { get; set; } = string.Empty;
  public string AgentLastName { get; set; } = string.Empty;
  public string? AvatarUrl { get; set; }
  public string? CompanyName { get; set; }
}
