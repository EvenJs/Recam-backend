using MongoDB.Driver;
using Remp.DataAccess.Collections;
using Remp.Models.MongoDocuments;
using Remp.Repository.Interfaces;

namespace Remp.Repository.Repositories;

public class CaseHistoryRepository : ICaseHistoryRepository
{
  private readonly IMongoCollection<CaseHistory> _collection;

  public CaseHistoryRepository(MongoDbContext mongoDbContext)
  {
    _collection = mongoDbContext.CaseHistory;
  }

  public async Task InsertAsync(CaseHistory document) 
    => await _collection.InsertOneAsync(document);

  public async Task<IEnumerable<CaseHistory>> GetByListingIdAsync(int listingCaseId)
    => await _collection
      .Find(x => x.ListingCaseId == listingCaseId)
      .ToListAsync();
}