using Remp.Models.MongoDocuments;

namespace Remp.Repository.Interfaces;

public interface ICaseHistoryRepository
{
  Task InsertAsync(CaseHistory document);
  Task <IEnumerable<CaseHistory>> GetByListingIdAsync(int listingCaseId);
}