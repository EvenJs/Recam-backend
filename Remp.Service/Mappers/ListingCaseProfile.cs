using AutoMapper;
using Remp.Models.Entities;
using Remp.Service.DTOs.ListingCase;

namespace Remp.Service.Mappers;

public class ListingCaseProfile : Profile
{
  public ListingCaseProfile()
  {
    CreateMap<CreateListingCaseRequest, ListingCase>();
    CreateMap<UpdateListingCaseRequest, ListingCase>()
      .ForAllMembers(opts => opts.Condition((stc, dest, srcMember) => srcMember != null));
    CreateMap<ListingCase, ListingCaseResponse>();
  }
}
