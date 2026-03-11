using Remp.Models.Entities;

namespace Remp.Repository.Interfaces;

public interface ICaseContactRepository : IBaseRepository<CaseContact>
{
  Task<IEnumerable<CaseContact>> GetByListingIdAsync(int listingCaseId);
}