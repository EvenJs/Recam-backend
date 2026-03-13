using AutoMapper;
using Remp.Models.Entities;
using Remp.Service.DTOs.CaseContact;

namespace Remp.Service.Mappers;

public class CaseContactProfile : Profile
{
  public CaseContactProfile()
  {
    CreateMap<AddCaseContactRequest, CaseContact>();
    CreateMap<CaseContact, CaseContactResponse>();
  }
}
