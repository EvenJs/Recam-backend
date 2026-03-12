using Microsoft.AspNetCore.Http;
using Remp.Models.Enums;

namespace Remp.Service.DTOs.MediaAsset;

public class UploadMediaRequest
{
  public List<IFormFile> Files { get; set; } = [];
  public MediaType MediaType { get; set; }
}
