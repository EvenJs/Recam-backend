
namespace Remp.Models.Entities;

public class PhotographyCompany : ApplicationUser
{
  public string PhotographyCompanyName { get; set; } = string.Empty;
  
  public ICollection<ListingCase> ListingCases { get; set; } = new List<ListingCase>();

  public ICollection<AgentPhotographyCompany> AgentPhotographyCompanies { get; set; } = new List<AgentPhotographyCompany>();
}