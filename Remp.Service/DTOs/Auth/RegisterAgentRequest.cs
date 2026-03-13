
namespace Remp.Service.DTOs.Auth;

public class RegisterAgentRequest
{
  public string Email { get; set; } = string.Empty;
  public string AgentFirstName { get; set; } = string.Empty;
  public string AgentLastName { get; set; } = string.Empty;
  public string? CompanyName { get; set; }
}
