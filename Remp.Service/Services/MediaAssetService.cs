using AutoMapper;
using Remp.Common.Exceptions;
using Remp.Models.Entities;
using Remp.Models.MongoDocuments;
using Remp.Repository.Common;
using Remp.Service.DTOs.MediaAsset;
using Remp.Service.Interfaces;

namespace Remp.Service.Services;

public class MediaAssetService : IMediaAssetService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IBlobStorageService _blobStorageService;
  private readonly IMapper _mapper;

  public MediaAssetService(
    IUnitOfWork unitOfWork,
    IBlobStorageService blobStorageService,
    IMapper mapper)
  {
    _unitOfWork = unitOfWork;
    _blobStorageService = blobStorageService;
    _mapper = mapper;
  }

  public async Task<IEnumerable<MediaAssetResponse>> UploadMediaAsync(
    int listingCaseId, UploadMediaRequest request, string userId)
  {
    var listing = await _unitOfWork.ListingCases.GetByIdAsync(listingCaseId)
      ?? throw new NotFoundException($"Listing case {listingCaseId} not found.");

    var responses = new List<MediaAssetResponse>();

    await _unitOfWork.BeginTransactionAsync();
    try
    {
      foreach (var file in request.Files)
      {
        await using var stream = file.OpenReadStream();
        var blobUrl = await _blobStorageService.UploadAsync(
          stream, file.FileName, file.ContentType);

        var mediaAsset = new MediaAsset
        {
          ListingCaseId = listingCaseId,
          UserId = userId,
          MediaType = request.MediaType,
          MediaUrl = blobUrl,
          UploadedAt = DateTime.UtcNow
        };

        await _unitOfWork.MediaAssets.AddAsync(mediaAsset);
        await _unitOfWork.SaveChangesAsync();
        responses.Add(_mapper.Map<MediaAssetResponse>(mediaAsset));
      }

      await _unitOfWork.CommitTransactionAsync();
    }
    catch
    {
      await _unitOfWork.RollbackTransactionAsync();
      throw;
    }

    return responses;
  }

  public async Task<IEnumerable<MediaAssetResponse>> GetMediaByListingIdAsync(int listingCaseId)
  {
    var media = await _unitOfWork.MediaAssets.GetByListingIdAsync(listingCaseId);
    return _mapper.Map<IEnumerable<MediaAssetResponse>>(media);
  }

  public async Task DeleteMediaAsync(int mediaId, string userId)
  {
    var media = await _unitOfWork.MediaAssets.GetByIdAsync(mediaId)
      ?? throw new NotFoundException($"Media asset {mediaId} not found.");

    await _blobStorageService.DeleteAsync(media.MediaUrl);
    media.IsDeleted = true;
    await _unitOfWork.SaveChangesAsync();
  }

  public async Task SetHeroImageAsync(int listingCaseId, int mediaId)
  {
    var media = await _unitOfWork.MediaAssets.GetByIdAsync(mediaId)
      ?? throw new NotFoundException($"Media asset {mediaId} not found.");

    if (media.ListingCaseId != listingCaseId)
      throw new BadRequestException("Media does not belong to this listing.");

    var currentHero = await _unitOfWork.MediaAssets.GetHeroByListingIdAsync(listingCaseId);
    if (currentHero != null)
    {
      currentHero.IsHero = false;
      await _unitOfWork.MediaAssets.UpdateAsync(currentHero);
    }

    media.IsHero = true;
    await _unitOfWork.MediaAssets.UpdateAsync(media);
    await _unitOfWork.SaveChangesAsync();
  }

  public async Task<IEnumerable<MediaAssetResponse>> GetSelectedMediaAsync(int listingCaseId)
  {
    var media = await _unitOfWork.MediaAssets.GetSelectedByListingIdAsync(listingCaseId);
    return _mapper.Map<IEnumerable<MediaAssetResponse>>(media);
  }

  public async Task UpdateSelectedMediaAsync(int listingCaseId, List<int> mediaIds, string userId)
  {
    if (mediaIds.Count > 10)
      throw new BadRequestException("A maximum of 10 images can be selected.");

    var allMedia = await _unitOfWork.MediaAssets.GetByListingIdAsync(listingCaseId);

    await _unitOfWork.BeginTransactionAsync();
    try
    {
      foreach (var asset in allMedia)
      {
        asset.IsSelect = mediaIds.Contains(asset.Id);
        await _unitOfWork.MediaAssets.UpdateAsync(asset);
      }

      await _unitOfWork.SaveChangesAsync();

      await _unitOfWork.UserActivityLogs.InsertAsync(new UserActivityLog
      {
        UserId = userId,
        Action = "UpdatedSelectedMedia",
        Details = $"Selected {mediaIds.Count} media items for listing {listingCaseId}",
        CreatedAt = DateTime.UtcNow
      });

      await _unitOfWork.CommitTransactionAsync();
    }
    catch
    {
      await _unitOfWork.RollbackTransactionAsync();
      throw;
    }
  }
}