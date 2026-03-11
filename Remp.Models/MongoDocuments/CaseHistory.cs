using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Remp.Models.MongoDocuments;

public class CaseHistory
{
  [BsonId]
  [BsonRepresentation(BsonType.ObjectId)]
  public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

  public int ListingCaseId { get; set; }
  public string OperatorId { get; set; } = string.Empty;
  public string Action { get; set; } = string.Empty;
  public string? FieldChanged { get; set; }
  public string? OldValue { get; set; }
  public string? NewValue { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}