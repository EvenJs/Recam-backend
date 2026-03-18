using AutoMapper;
using Moq;
using Remp.Common.Exceptions;
using Remp.Models.Entities;
using Remp.Repository.Common;
using Remp.Repository.Interfaces;
using Remp.Service.DTOs.CaseContact;
using Remp.Service.Services;

namespace Remp.Tests;

public class CaseContactServiceTests
{
  private readonly Mock<IUnitOfWork> _unitOfWorkMock;
  private readonly Mock<IMapper> _mapperMock;
  private readonly Mock<ICaseContactRepository> _contactRepoMock;
  private readonly Mock<IListingCaseRepository> _listingRepoMock;
  private readonly CaseContactService _service;

  public CaseContactServiceTests()
  {
    _unitOfWorkMock = new Mock<IUnitOfWork>();
    _mapperMock = new Mock<IMapper>();
    _contactRepoMock = new Mock<ICaseContactRepository>();
    _listingRepoMock = new Mock<IListingCaseRepository>();

    _unitOfWorkMock.Setup(u => u.CaseContacts).Returns(_contactRepoMock.Object);
    _unitOfWorkMock.Setup(u => u.ListingCases).Returns(_listingRepoMock.Object);
    _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

    _service = new CaseContactService(_unitOfWorkMock.Object, _mapperMock.Object);
  }
  [Fact]
  public async Task AddContactAsync_ListingNotFound_ThrowsNotFoundException()
  {
    // Arrange
    _listingRepoMock
        .Setup(r => r.GetByIdAsync(99))
        .ReturnsAsync((ListingCase?)null);

    var request = new AddCaseContactRequest
    {
      FirstName = "John",
      LastName = "Doe",
      Email = "john@test.com"
    };

    // Act & Assert
    await Assert.ThrowsAsync<NotFoundException>(
        () => _service.AddContactAsync(99, request));
  }

  [Fact]
  public async Task AddContactAsync_ValidRequest_ReturnsContactResponse()
  {
    // Arrange
    var listing = new ListingCase { Id = 1, Title = "Test Listing" };
    var contact = new CaseContact { ContactId = 1, FirstName = "John" };
    var response = new CaseContactResponse { ContactId = 1, FirstName = "John" };

    _listingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(listing);
    _mapperMock.Setup(m => m.Map<CaseContact>(It.IsAny<AddCaseContactRequest>())).Returns(contact);
    _mapperMock.Setup(m => m.Map<CaseContactResponse>(contact)).Returns(response);
    _contactRepoMock.Setup(r => r.AddAsync(It.IsAny<CaseContact>())).Returns(Task.CompletedTask);

    var request = new AddCaseContactRequest
    {
      FirstName = "John",
      LastName = "Doe",
      Email = "john@test.com"
    };

    // Act
    var result = await _service.AddContactAsync(1, request);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(1, result.ContactId);
    Assert.Equal("John", result.FirstName);
  }

  [Fact]
  public async Task GetContactsByListingIdAsync_ReturnsContacts()
  {
    // Arrange
    var contacts = new List<CaseContact>
        {
            new CaseContact { ContactId = 1, FirstName = "John" },
            new CaseContact { ContactId = 2, FirstName = "Jane" }
        };

    var responses = new List<CaseContactResponse>
        {
            new CaseContactResponse { ContactId = 1, FirstName = "John" },
            new CaseContactResponse { ContactId = 2, FirstName = "Jane" }
        };

    _contactRepoMock.Setup(r => r.GetByListingIdAsync(1)).ReturnsAsync(contacts);
    _mapperMock.Setup(m => m.Map<IEnumerable<CaseContactResponse>>(contacts)).Returns(responses);

    // Act
    var result = await _service.GetContactsByListingIdAsync(1);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(2, result.Count());
  }

}
