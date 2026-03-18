using AutoMapper;
using Moq;
using Remp.Common.Exceptions;
using Remp.Models.Entities;
using Remp.Models.Enums;
using Remp.Repository.Common;
using Remp.Repository.Interfaces;
using Remp.Service.Interfaces;
using Remp.Service.DTOs.MediaAsset;
using Remp.Service.Services;

namespace Remp.Tests;

public class MediaAssetServiceTests
{
  private readonly Mock<IUnitOfWork> _unitOfWorkMock;
  private readonly Mock<IBlobStorageService> _blobServiceMock;
  private readonly Mock<IMapper> _mapperMock;
  private readonly Mock<IMediaAssetRepository> _mediaRepoMock;
  private readonly Mock<IListingCaseRepository> _listingRepoMock;
  private readonly MediaAssetService _service;

  public MediaAssetServiceTests()
  {
    _unitOfWorkMock = new Mock<IUnitOfWork>();
    _blobServiceMock = new Mock<IBlobStorageService>();
    _mapperMock = new Mock<IMapper>();
    _mediaRepoMock = new Mock<IMediaAssetRepository>();
    _listingRepoMock = new Mock<IListingCaseRepository>();

    _unitOfWorkMock.Setup(u => u.MediaAssets).Returns(_mediaRepoMock.Object);
    _unitOfWorkMock.Setup(u => u.ListingCases).Returns(_listingRepoMock.Object);
    _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);
    _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
    _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
    _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

    _service = new MediaAssetService(
        _unitOfWorkMock.Object,
        _blobServiceMock.Object,
        _mapperMock.Object);
  }

  [Fact]
  public async Task DeleteMediaAsync_NotFound_ThrowsNotFoundException()
  {
    // Arrange
    _mediaRepoMock
        .Setup(r => r.GetByIdAsync(99))
        .ReturnsAsync((MediaAsset?)null);

    // Act & Assert
    await Assert.ThrowsAsync<NotFoundException>(
        () => _service.DeleteMediaAsync(99, "user-123"));
  }

  [Fact]
  public async Task DeleteMediaAsync_ValidId_SoftDeletesMedia()
  {
    // Arrange
    var media = new MediaAsset
    {
      Id = 1,
      MediaUrl = "https://blob.core.windows.net/file.jpg",
      IsDeleted = false
    };

    _mediaRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(media);
    _blobServiceMock
        .Setup(b => b.DeleteAsync(It.IsAny<string>()))
        .Returns(Task.CompletedTask);

    // Act
    await _service.DeleteMediaAsync(1, "user-123");

    // Assert
    Assert.True(media.IsDeleted);
  }

  [Fact]
  public async Task SetHeroImageAsync_MediaNotBelongToListing_ThrowsBadRequestException()
  {
    // Arrange
    var media = new MediaAsset
    {
      Id = 1,
      ListingCaseId = 99 // belongs to different listing
    };

    _mediaRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(media);

    // Act & Assert — passing listingId=1 but media belongs to 99
    await Assert.ThrowsAsync<BadRequestException>(
        () => _service.SetHeroImageAsync(1, 1));
  }

  [Fact]
  public async Task UpdateSelectedMediaAsync_ExceedsMaxLimit_ThrowsBadRequestException()
  {
    // Arrange — 11 items exceeds max of 10
    var mediaIds = Enumerable.Range(1, 11).ToList();

    // Act & Assert
    await Assert.ThrowsAsync<BadRequestException>(
        () => _service.UpdateSelectedMediaAsync(1, mediaIds, "user-123"));
  }

  [Fact]
  public async Task GetMediaByListingIdAsync_ReturnsMediaList()
  {
    // Arrange
    var mediaList = new List<MediaAsset>
        {
            new MediaAsset { Id = 1, MediaType = MediaType.Picture },
            new MediaAsset { Id = 2, MediaType = MediaType.Video }
        };

    var responses = new List<MediaAssetResponse>
        {
            new MediaAssetResponse { Id = 1 },
            new MediaAssetResponse { Id = 2 }
        };

    _mediaRepoMock.Setup(r => r.GetByListingIdAsync(1)).ReturnsAsync(mediaList);
    _mapperMock
        .Setup(m => m.Map<IEnumerable<MediaAssetResponse>>(mediaList))
        .Returns(responses);

    // Act
    var result = await _service.GetMediaByListingIdAsync(1);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(2, result.Count());
  }
}
