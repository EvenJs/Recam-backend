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
        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    CreateMap<ListingCase, ListingCaseResponse>()
        .ForMember(
            dest => dest.MediaTypes,
            opt => opt.MapFrom(src =>
                src.MediaAssets
                   .Select(m => (int)m.MediaType)
                   .Distinct()
                   .OrderBy(x => x)
                   .ToList()));
  }
}
