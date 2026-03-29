using AutoMapper;
using Microsoft.Extensions.Configuration;
using Remp.Common.Exceptions;
using Remp.Models.Entities;
using Remp.Models.Enums;
using Remp.Models.MongoDocuments;
using Remp.Repository.Common;
using Remp.Service.DTOs.CaseContact;
using Remp.Service.DTOs.ListingCase;
using Remp.Service.DTOs.MediaAsset;
using Remp.Service.Interfaces;

namespace Remp.Service.Services;

public class ListingCaseService : IListingCaseService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;
  private readonly IConfiguration _configuration;

  public ListingCaseService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
  {
    _unitOfWork = unitOfWork;
    _mapper = mapper;
    _configuration = configuration;
  }

  public async Task<ListingCaseResponse> CreateListingCaseAsync(CreateListingCaseRequest request, string userId)
  {
    var listingCase = _mapper.Map<ListingCase>(request);
    listingCase.UserId = userId;
    listingCase.ListcaseStatus = ListcaseStatus.Created;
    listingCase.CreatedAt = DateTime.UtcNow;

    await _unitOfWork.BeginTransactionAsync();
    try
    {
      await _unitOfWork.ListingCases.AddAsync(listingCase);
      await _unitOfWork.SaveChangesAsync();

      await _unitOfWork.CaseHistories.InsertAsync(new CaseHistory
      {
        ListingCaseId = listingCase.Id,
        OperatorId = userId,
        Action = "Created",
        CreatedAt = DateTime.UtcNow
      });
      await _unitOfWork.CommitTransactionAsync();
    }
    catch
    {
      await _unitOfWork.RollbackTransactionAsync();
      throw;
    }

    return _mapper.Map<ListingCaseResponse>(listingCase);
  }

  public async Task<(IEnumerable<ListingCaseResponse> Items, int TotalCount)> GetListingsAsync(
    string? userId,
    string? agentId,
    int? statusFilter,
    int page,
    int pageSize)
  {
    var (items, totalCount) = await _unitOfWork.ListingCases.GetPagedAsync(userId, agentId, statusFilter, page, pageSize);

    return (_mapper.Map<IEnumerable<ListingCaseResponse>>(items), totalCount);
  }

  public async Task<ListingCaseResponse> GetListingByIdAsync(int id)
  {
    var listing = await _unitOfWork.ListingCases.GetByIdAsync(id)
      ?? throw new NotFoundException($"Listing case {id} not found.");

    return _mapper.Map<ListingCaseResponse>(listing);
  }

  public async Task<ListingCaseResponse> UpdateListingCaseAsync(int id, UpdateListingCaseRequest request)
  {
    var listing = await _unitOfWork.ListingCases.GetByIdAsync(id)
      ?? throw new NotFoundException($"Listing case {id} not found.");

    _mapper.Map(request, listing);
    await _unitOfWork.SaveChangesAsync();

    return _mapper.Map<ListingCaseResponse>(listing);
  }

  public async Task DeleteListingCaseAsync(int id)
  {
    var listing = await _unitOfWork.ListingCases.GetByIdAsync(id)
      ?? throw new NotFoundException($"Listing case {id} not found.");

    listing.IsDeleted = true;
    await _unitOfWork.SaveChangesAsync();
  }

  public async Task UpdateListingStatusAsync(int id, int newStatus, string operatorId)
  {
    var listing = await _unitOfWork.ListingCases.GetByIdAsync(id)
      ?? throw new NotFoundException($"Listing case {id} not found.");

    var currentStatus = (int)listing.ListcaseStatus;

    if (newStatus != currentStatus + 1)
      throw new BadRequestException(
        $"Invalid status transition. Cannot move from {listing.ListcaseStatus} to status {newStatus}.");

    var oldStatus = listing.ListcaseStatus.ToString();
    listing.ListcaseStatus = (ListcaseStatus)newStatus;

    await _unitOfWork.BeginTransactionAsync();
    try
    {
      await _unitOfWork.SaveChangesAsync();

      await _unitOfWork.CaseHistories.InsertAsync(new CaseHistory
      {
        ListingCaseId = id,
        OperatorId = operatorId,
        Action = "StatusUpdated",
        FieldChanged = "ListcaseStatus",
        OldValue = oldStatus,
        NewValue = listing.ListcaseStatus.ToString(),
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

  public async Task AssignAgentToListingAsync(int listingId, string agentId, string operatorId)
  {
    var listing = await _unitOfWork.ListingCases.GetByIdAsync(listingId)
      ?? throw new NotFoundException($"Listing case {listingId} not found.");

    var alreadyAssigned = await _unitOfWork.AgentListingCases.ExistsAsync(agentId, listingId);
    if (alreadyAssigned)
      throw new ConflictException("Agent is already assigned to this listing.");

    var assignment = new AgentListingCase
    {
      AgentId = agentId,
      ListingCaseId = listingId
    };

    await _unitOfWork.BeginTransactionAsync();
    try
    {
      await _unitOfWork.AgentListingCases.AddAsync(assignment);
      await _unitOfWork.SaveChangesAsync();

      await _unitOfWork.UserActivityLogs.InsertAsync(new UserActivityLog
      {
        UserId = operatorId,
        Action = "AssignedAgent",
        Details = $"Agent {agentId} assigned to listing {listingId}",
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

  public async Task<string> PublishListingAsync(int listingId, string operatorId)
  {
    var listing = await _unitOfWork.ListingCases.GetByIdAsync(listingId)
        ?? throw new NotFoundException($"Listing case {listingId} not found.");

    // Return existing URL if already published
    if (!string.IsNullOrEmpty(listing.ShareableUrl))
      return listing.ShareableUrl;

    var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
        .Replace("/", "-")
        .Replace("+", "_")
        .Replace("=", "")
        .ToLower();

    var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
    listing.ShareableUrl = $"{frontendUrl}/p/{token}";

    await _unitOfWork.BeginTransactionAsync();
    try
    {
      await _unitOfWork.SaveChangesAsync();

      await _unitOfWork.CaseHistories.InsertAsync(new CaseHistory
      {
        ListingCaseId = listingId,
        OperatorId = operatorId,
        Action = "Published",
        FieldChanged = "ShareableUrl",
        NewValue = listing.ShareableUrl,
        CreatedAt = DateTime.UtcNow
      });

      await _unitOfWork.CommitTransactionAsync();
    }
    catch
    {
      await _unitOfWork.RollbackTransactionAsync();
      throw;
    }

    return listing.ShareableUrl;
  }

  public async Task<ListingCasePreviewResponse> GetListingByTokenAsync(string token)
  {
    var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
    var shareableUrl = $"{frontendUrl}/p/{token}";

    var listing = await _unitOfWork.ListingCases.GetByShareableUrlAsync(shareableUrl)
        ?? throw new NotFoundException("Listing not found.");

    var response = _mapper.Map<ListingCasePreviewResponse>(listing);

    // Hero image
    var heroImage = await _unitOfWork.MediaAssets.GetHeroByListingIdAsync(listing.Id);
    response.HeroImage = heroImage != null ? _mapper.Map<MediaAssetResponse>(heroImage) : null;

    // Selected media
    var selectedMedia = await _unitOfWork.MediaAssets.GetSelectedByListingIdAsync(listing.Id);
    response.SelectedMedia = _mapper.Map<List<MediaAssetResponse>>(selectedMedia);

    // Contacts
    var contacts = await _unitOfWork.CaseContacts.GetByListingIdAsync(listing.Id);
    response.Contacts = _mapper.Map<List<CaseContactResponse>>(contacts);

    return response;
  }
}
