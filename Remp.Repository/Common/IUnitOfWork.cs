using Remp.Repository.Interfaces;

namespace Remp.Repository.Common;

public interface IUnitOfWork : IDisposable
{
  IListingCaseRepository ListingCases { get; }
  IMediaAssetRepository MediaAssets { get; }
  IAgentRepository Agents { get; }
  ICaseContactRepository CaseContacts { get; }
  IAgentListingCaseRepository AgentListingCases { get; }
  IAgentPhotographyCompanyRepository AgentPhotographyCompanies { get; }
  ICaseHistoryRepository CaseHistories { get; }
  IUserActivityLogRepository UserActivityLogs { get; }

  Task<int> SaveChangesAsync();
  Task BeginTransactionAsync();
  Task CommitTransactionAsync();
  Task RollbackTransactionAsync();
}