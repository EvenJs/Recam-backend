using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using Remp.Service.Interfaces;

namespace Remp.Service.Services;

public class BlobStorageService : IBlobStorageService
{
  private readonly BlobContainerClient _containerClient;
  private readonly BlobServiceClient _blobServiceClient;

  public BlobStorageService(IConfiguration configuration)
  {
    var connectionString = configuration["BlobStorageSettings:ConnectionString"]
        ?? throw new InvalidOperationException("Blob Storage connection string is not configured.");

    var containerName = configuration["BlobStorageSettings:ContainerName"]
        ?? throw new InvalidOperationException("Blob Storage container name is not configured.");

    _blobServiceClient = new BlobServiceClient(connectionString);
    _containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
    _containerClient.CreateIfNotExists(PublicAccessType.None);
  }

  public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
  {
    var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
    var blobClient = _containerClient.GetBlobClient(uniqueFileName);

    await blobClient.UploadAsync(fileStream, new BlobHttpHeaders
    {
      ContentType = contentType
    });

    // Return SAS URL valid for 1 year
    return GenerateSasUrl(blobClient);
  }

  public async Task DeleteAsync(string blobUrl)
  {
    var blobName = ExtractBlobName(blobUrl);
    var blobClient = _containerClient.GetBlobClient(blobName);
    await blobClient.DeleteIfExistsAsync();
  }

  public async Task<(Stream FileStream, string ContentType, string FileName)> DownloadAsync(string blobUrl)
  {
    var blobName = ExtractBlobName(blobUrl);
    var blobClient = _containerClient.GetBlobClient(blobName);

    var response = await blobClient.DownloadStreamingAsync();
    var contentType = response.Value.Details.ContentType ?? "application/octet-stream";

    return (response.Value.Content, contentType, blobName);
  }

  private string GenerateSasUrl(BlobClient blobClient)
  {
    var sasBuilder = new BlobSasBuilder
    {
      BlobContainerName = _containerClient.Name,
      BlobName = blobClient.Name,
      Resource = "b",
      ExpiresOn = DateTimeOffset.UtcNow.AddYears(1)
    };

    sasBuilder.SetPermissions(BlobSasPermissions.Read);

    return blobClient.GenerateSasUri(sasBuilder).ToString();
  }

  private string ExtractBlobName(string blobUrl)
  {
    var uri = new Uri(blobUrl);
    var path = uri.LocalPath;
    var containerName = _containerClient.Name;
    var index = path.IndexOf(containerName, StringComparison.OrdinalIgnoreCase);
    return index >= 0 ? path[(index + containerName.Length + 1)..] : Path.GetFileName(path);
  }
}