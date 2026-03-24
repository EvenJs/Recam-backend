using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Remp.Service.Interfaces;

namespace Remp.Service.Services;

public class BlobStorageService : IBlobStorageService
{
  private readonly BlobContainerClient _containerClient;

  public BlobStorageService(IConfiguration configuration)
  {
    var connectionString = configuration["BlobStorageSettings:ConnectionString"]
      ?? throw new InvalidOperationException("Azure Blob Storage connection string is not configured.");

    var containerName = configuration["BlobStorageSettings:ContainerName"]
      ?? throw new InvalidOperationException("Azure Blob Storage container name is not configured.");

    var blobServiceClient = new BlobServiceClient(connectionString);
    _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    _containerClient.CreateIfNotExists(PublicAccessType.Blob);
  }

  public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
  {
    var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
    var blobClient = _containerClient.GetBlobClient(uniqueFileName);

    await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });

    return blobClient.Uri.ToString();
  }

  public async Task DeleteAsync(string blobUrl)
  {
    var blobName = Path.GetFileName(new Uri(blobUrl).LocalPath);
    var blobClient = _containerClient.GetBlobClient(blobName);
    await blobClient.DeleteIfExistsAsync();
  }

  public async Task<(Stream FileStream, string ContentType, string FileName)> DownloadAsync(string blobUrl)
  {
    var blobName = Path.GetFileName(new Uri(blobUrl).LocalPath);
    var blobClient = _containerClient.GetBlobClient(blobName);

    var response = await blobClient.DownloadStreamingAsync();
    var contentType = response.Value.Details.ContentType ?? "application/octet-stream";

    return (response.Value.Content, contentType, blobName);
  }
}
