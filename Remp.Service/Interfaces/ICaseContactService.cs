using Remp.Service.DTOs.CaseContact;

namespace Remp.Service.Interfaces;

public interface ICaseContactService
{
  Task<CaseContactResponse> AddContactAsync(int listingCaseId, AddCaseContactRequest request);
  Task<IEnumerable<CaseContactResponse>> GetContactsByListingIdAsync(int listingCaseId);
}
