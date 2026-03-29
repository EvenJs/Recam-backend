using Remp.Models.Entities;

namespace Remp.Repository.Interfaces;

public interface IListingCaseRepository : IBaseRepository<ListingCase>
{
  Task<IEnumerable<ListingCase>> GetByUserIdAsync(string userId);
  Task<IEnumerable<ListingCase>> GetByAgentIdAsync(string agentId);
  Task<(IEnumerable<ListingCase> Items, int TotalCount)> GetPagedAsync(
    string? userId,
    string? agentId,
    int? statusFilter,
    int page,
    int pageSize);
  Task<ListingCase?> GetByShareableUrlAsync(string shareableUrl);
}