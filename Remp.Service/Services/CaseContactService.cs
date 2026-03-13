using AutoMapper;
using Remp.Common.Exceptions;
using Remp.Models.Entities;
using Remp.Repository.Common;
using Remp.Service.DTOs.CaseContact;
using Remp.Service.Interfaces;

namespace Remp.Service.Services;

public class CaseContactService : ICaseContactService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;

  public CaseContactService(IUnitOfWork unitOfWork, IMapper mapper)
  {
    _unitOfWork = unitOfWork;
    _mapper = mapper;
  }

  public async Task<CaseContactResponse> AddContactAsync(int listingCaseId, AddCaseContactRequest request)
  {
    var listing = await _unitOfWork.ListingCases.GetByIdAsync(listingCaseId)
      ?? throw new NotFoundException($"Listing case {listingCaseId} not found.");

    var contact = _mapper.Map<CaseContact>(request);
    contact.ListingCaseId = listingCaseId;

    await _unitOfWork.CaseContacts.AddAsync(contact);
    await _unitOfWork.SaveChangesAsync();

    return _mapper.Map<CaseContactResponse>(contact);
  }

  public async Task<IEnumerable<CaseContactResponse>> GetContactsByListingIdAsync(int listingCaseId)
  {
    var contacts = await _unitOfWork.CaseContacts.GetByListingIdAsync(listingCaseId);
    return _mapper.Map<IEnumerable<CaseContactResponse>>(contacts);
}
}