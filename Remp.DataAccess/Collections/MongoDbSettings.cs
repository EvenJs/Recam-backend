
namespace Remp.DataAccess.Collections;

public class MongoDbSettings
{
  public string ConnectionString { get; set; } = string.Empty;
  public string DatabaseName { get; set; } = string.Empty;
  public string CaseHistoryCollection { get; set; } = string.Empty;
  public string UserActivityLogCollection { get; set; } = string.Empty;
}