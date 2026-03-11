using Remp.Models.MongoDocuments;

namespace Remp.Repository.Interfaces;

public interface IUserActivityLogRepository
{
  Task InsertAsync(UserActivityLog document);
  Task<IEnumerable<UserActivityLog>> GetByUserIdAsync(string userId);
}