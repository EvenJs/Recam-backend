using AutoMapper;
using Microsoft.Extensions.Configuration;
using Moq;
using Remp.Common.Exceptions;
using Remp.Models.Entities;
using Remp.Models.Enums;
using Remp.Repository.Common;
using Remp.Repository.Interfaces;
using Remp.Service.DTOs;
using Remp.Service.DTOs.ListingCase;
using Remp.Service.Services;

namespace Remp.Tests;

public class ListingCaseServiceTests
{
  private readonly Mock<IUnitOfWork> _unitOfWorkMock;
  private readonly Mock<AutoMapper.IMapper> _mapperMock;
  private readonly Mock<IListingCaseRepository> _listingRepoMock;
  private readonly ListingCaseService _service;
  private readonly Mock<IConfiguration> _configurationMock;

  public ListingCaseServiceTests()
  {
    _unitOfWorkMock = new Mock<IUnitOfWork>();
    _mapperMock = new Mock<AutoMapper.IMapper>();
    _listingRepoMock = new Mock<IListingCaseRepository>();
    _configurationMock = new Mock<IConfiguration>();



    _unitOfWorkMock.Setup(u => u.ListingCases).Returns(_listingRepoMock.Object);
    _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
    _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
    _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);
    _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);
    _configurationMock.Setup(c => c["FrontendUrl"]).Returns("http://localhost:5173");

    _service = new ListingCaseService(_unitOfWorkMock.Object, _mapperMock.Object, _configurationMock.Object);
  }

  [Fact]
  public async Task CreateListingCaseAsync_ValidRequest_ReturnsListingCaseResponse()
  {
    var request = new CreateListingCaseRequest
    {
      Title = "Test Listing",
      Street = "123 Test st",
      City = "Melbourne",
      State = "VIC",
      Postcode = 3000
    };

    var listingCase = new ListingCase
    {
      Id = 1,
      Title = "Test Listing",
      UserId = "user-123",
      ListcaseStatus = ListcaseStatus.Created
    };

    var response = new ListingCaseResponse { Id = 1, Title = "Test Listing" };

    _mapperMock.Setup(m => m.Map<ListingCase>(request)).Returns(listingCase);
    _mapperMock.Setup(m => m.Map<ListingCaseResponse>(listingCase)).Returns(response);

    _unitOfWorkMock.Setup(u => u.CaseHistories.InsertAsync(It.IsAny<Remp.Models.MongoDocuments.CaseHistory>())).Returns(Task.CompletedTask);

    var result = await _service.CreateListingCaseAsync(request, "user-123");

    Assert.NotNull(result);
    Assert.Equal(1, result.Id);
    Assert.Equal("Test Listing", result.Title);
  }

  [Fact]
  public async Task GetListingByIdAsync_ExistingId_ReturnsListingCaseResponse()
  {
    var listingCase = new ListingCase { Id = 1, Title = "Test Listing" };
    var response = new ListingCaseResponse { Id = 1, Title = "Test Listing" };

    _listingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(listingCase);
    _mapperMock.Setup(m => m.Map<ListingCaseResponse>(listingCase)).Returns(response);

    var result = await _service.GetListingByIdAsync(1);

    Assert.NotNull(result);
    Assert.Equal(1, result.Id);
  }

  [Fact]
  public async Task GetListingByIdAsync_NotFound_ThrowsNotFoundException()
  {
    _listingRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ListingCase?)null);

    await Assert.ThrowsAsync<NotFoundException>(() => _service.GetListingByIdAsync(99));
  }

  [Fact]
  public async Task GetListingStatusAsync_ValidTransition_UpdatesStatus()
  {
    var listingCase = new ListingCase
    {
      Id = 1,
      ListcaseStatus = ListcaseStatus.Created
    };

    _listingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(listingCase);

    _unitOfWorkMock.Setup(u => u.CaseHistories.InsertAsync(It.IsAny<Remp.Models.MongoDocuments.CaseHistory>())).Returns(Task.CompletedTask);

    await _service.UpdateListingStatusAsync(1, 2, "operator-123");

    Assert.Equal(ListcaseStatus.Pending, listingCase.ListcaseStatus);
  }

  [Fact]
  public async Task UpdateListingStatusAsync_InvalidTransition_ThrowsBadRequestException()
  {
    var listingCase = new ListingCase
    {
      Id = 1,
      ListcaseStatus = ListcaseStatus.Created
    };

    _listingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(listingCase);

    await Assert.ThrowsAsync<BadRequestException>(() => _service.UpdateListingStatusAsync(1, 3, "operator-123"));
  }

}
