
namespace Remp.Service.Interfaces;

public interface IBlobStorageService
{
  Task<string> UploadAsync(Stream fileStream, string fileName, string contentType);
  Task DeleteAsync(string blobUrl);
  Task<(Stream FileStream, string ContentType, string FileName)> DownloadAsync(string blobUrl);
}
