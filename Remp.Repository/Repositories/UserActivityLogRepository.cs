using MongoDB.Driver;
using Remp.DataAccess.Collections;
using Remp.Models.MongoDocuments;
using Remp.Repository.Interfaces;

namespace Remp.Repository.Repositories;

public class UserActivityLogRepository : IUserActivityLogRepository
{
  public readonly IMongoCollection<UserActivityLog> _collection;

  public UserActivityLogRepository(MongoDbContext mongoDbContext)
  {
    _collection = mongoDbContext.UserActivityLog;
  }

  public async Task InsertAsync(UserActivityLog document) 
    => await _collection.InsertOneAsync(document);

  public async Task<IEnumerable<UserActivityLog>> GetByUserIdAsync(string userId) 
    => await _collection
      .Find(x => x.UserId == userId)
      .ToListAsync();
}