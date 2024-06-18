using AutoMapper;
using Druware.Server.Entities;
using Druware.Server.Models;

namespace Trustwin.FileService;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UserRegistrationModel, User>()
            .ForMember(u => u.UserName, opt => opt.MapFrom(x => x.Email));
    }
}