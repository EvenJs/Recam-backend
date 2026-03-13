using AutoMapper;
using Remp.Models.Entities;
using Remp.Service.DTOs.Agent;

namespace Remp.Service.Mappers;

public class AgentProfile : Profile
{
  public AgentProfile()
  {
    CreateMap<Agent, AgentResponse>()
      .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
  }
}
