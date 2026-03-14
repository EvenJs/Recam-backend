using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Remp.Models.MongoDocuments;

namespace Remp.DataAccess.Collections;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<CaseHistory> CaseHistory
        => _database.GetCollection<CaseHistory>("CaseHistory");

    public IMongoCollection<UserActivityLog> UserActivityLog
        => _database.GetCollection<UserActivityLog>("UserActivityLog");
}