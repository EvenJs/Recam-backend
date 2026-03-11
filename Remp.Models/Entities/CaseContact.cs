
namespace Remp.Models.Entities;

public class CaseContact
{
  public int ContactId { get; set; }
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;
  public string? CompanyName { get; set; }
  public string? ProfileUrl { get; set; }
  public string Email { get; set; } = string.Empty;
  public string PhoneNumber { get; set; } = string.Empty;

  public int ListingCaseId { get; set; }
  public ListingCase ListingCase { get; set; } = null!;
}