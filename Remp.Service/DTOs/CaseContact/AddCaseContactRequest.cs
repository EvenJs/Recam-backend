
namespace Remp.Service.DTOs.CaseContact;

public class AddCaseContactRequest
{
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;
  public string? CompanyName { get; set; }
  public string? ProfileUrl { get; set; }
  public string Email { get; set; } = string.Empty;
  public string? PhoneNumber { get; set; }
}
