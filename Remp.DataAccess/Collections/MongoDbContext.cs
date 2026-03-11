using MongoDB.Driver;
using Remp.Models.MongoDocuments;

namespace Remp.DataAccess.Collections;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
    }

    public IMongoCollection<CaseHistory> CaseHistory
        => _database.GetCollection<CaseHistory>("CaseHistory");

    public IMongoCollection<UserActivityLog> UserActivityLog
        => _database.GetCollection<UserActivityLog>("UserActivityLog");
}