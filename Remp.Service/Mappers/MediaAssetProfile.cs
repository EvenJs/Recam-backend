using AutoMapper;
using Remp.Models.Entities;
using Remp.Service.DTOs.MediaAsset;

namespace Remp.Service.Mappers;

public class MediaAssetProfile : Profile
{
  public MediaAssetProfile()
  {
    CreateMap<MediaAsset, MediaAssetResponse>();
  }
}
