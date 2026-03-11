using Microsoft.EntityFrameworkCore.Storage;
using Remp.DataAccess.Data;
using Remp.Repository.Interfaces;
using Remp.Repository.Repositories;

namespace Remp.Repository.Common;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public IListingCaseRepository ListingCases { get; }
    public IMediaAssetRepository MediaAssets { get; }
    public IAgentRepository Agents { get; }
    public ICaseContactRepository CaseContacts { get; }
    public IAgentListingCaseRepository AgentListingCases { get; }
    public IAgentPhotographyCompanyRepository AgentPhotographyCompanies { get; }
    public ICaseHistoryRepository CaseHistories { get; }
    public IUserActivityLogRepository UserActivityLogs { get; }

    public UnitOfWork(
        AppDbContext context,
        IListingCaseRepository listingCases,
        IMediaAssetRepository mediaAssets,
        IAgentRepository agents,
        ICaseContactRepository caseContacts,
        IAgentListingCaseRepository agentListingCases,
        IAgentPhotographyCompanyRepository agentPhotographyCompanies,
        ICaseHistoryRepository caseHistories,
        IUserActivityLogRepository userActivityLogs)
    {
        _context = context;
        ListingCases = listingCases;
        MediaAssets = mediaAssets;
        Agents = agents;
        CaseContacts = caseContacts;
        AgentListingCases = agentListingCases;
        AgentPhotographyCompanies = agentPhotographyCompanies;
        CaseHistories = caseHistories;
        UserActivityLogs = userActivityLogs;
    }

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task BeginTransactionAsync()
        => _transaction = await _context.Database.BeginTransactionAsync();

    public async Task CommitTransactionAsync()
    {
        if (_transaction is not null)
            await _transaction.CommitAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction is not null)
            await _transaction.RollbackAsync();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}