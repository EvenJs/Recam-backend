
namespace Remp.Models.Entities;

public class Agent : ApplicationUser
{
  public string AgentFirstName { get; set; } = string.Empty;
  public string AgentLastName { get; set; } = string.Empty;
  public string? AvatarUrl { get; set; }
  public string? CompanyName { get; set; }

  public ICollection<AgentPhotographyCompany> AgentPhotographyCompanies { get; set; } = new List<AgentPhotographyCompany>();
  public ICollection<AgentListingCase> AgentListingCases { get; set; } = new List<AgentListingCase>();
}