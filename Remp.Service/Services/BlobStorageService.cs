using Remp.Service.Interfaces;

namespace Remp.Service.Services;

public class BlobStorageService : IBlobStorageService
{
  public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
  {
    // TODO: implement Azure Blob Storage upload later
    await Task.CompletedTask;
    return $"https://placeholder.blob.core.windows.net/{fileName}";
  }

  public async Task DeleteAsync(string blobUrl)
  {
    // TODO: implement Azure Blob Storage delete later
    await Task.CompletedTask;
  }

  public async Task<(Stream FileStream, string ContentType, string FileName)> DownloadAsync(string blobUrl)
  {
    // TODO: implement Azure Blob Storage download later
    await Task.CompletedTask;
    return (Stream.Null, "application/octet-stream", "placeholder");
  }
}
