
namespace Remp.Models.Entities;

public class AgentListingCase
{
  public string AgentId { get; set; } = string.Empty;
  public int ListingCaseId { get; set; }

  public Agent Agent { get; set; } = null!;
  public ListingCase ListingCase { get; set; } = null!; 
}