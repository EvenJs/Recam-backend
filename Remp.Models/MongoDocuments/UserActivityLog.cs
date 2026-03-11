using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Remp.Models.MongoDocuments;

public class UserActivityLog
{
  [BsonId]
  [BsonRepresentation(BsonType.ObjectId)]
  public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

  public string UserId { get; set; } = string.Empty;
  public string Action { get; set; } = string.Empty;
  public string? Details { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}