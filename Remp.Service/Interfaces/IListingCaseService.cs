using Remp.Service.DTOs.ListingCase;

namespace Remp.Service.Interfaces;

public interface IListingCaseService
{
  Task<ListingCaseResponse> CreateListingCaseAsync(CreateListingCaseRequest request, string userId);
  Task<(IEnumerable<ListingCaseResponse> Items, int TotalCount)> GetListingsAsync(
      string? userId,
      string? agentId,
      int? statusFilter,
      int page,
      int pageSize);
  Task<ListingCaseResponse> GetListingByIdAsync(int id);
  Task<ListingCaseResponse> UpdateListingCaseAsync(int id, UpdateListingCaseRequest request);
  Task DeleteListingCaseAsync(int id);
  Task UpdateListingStatusAsync(int id, int newStatus, string operatorId);
  Task AssignAgentToListingAsync(int listingId, string agentId, string operatorId);
  Task<string> PublishListingAsync(int listingId, string operatorId);
}
