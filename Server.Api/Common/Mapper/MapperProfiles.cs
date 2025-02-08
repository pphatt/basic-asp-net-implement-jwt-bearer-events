using AutoMapper;
using Server.Application.Features.Authentication.Login;
using Server.Contracts.Authentication.Login;

namespace Server.Api.Common.Mapper;

public class MapperProfiles : Profile
{
    public MapperProfiles()
    {
        // Authentication
        CreateMap<LoginRequest, LoginCommand>();
    }
}
