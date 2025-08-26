using AutoMapper;
using InnovaSfera.Template.Application.Dto.Response;
using InnovaSfera.Template.Domain.Entities;

namespace InnovaSfera.Template.Application.Mapper;

public class AuthMapper : Profile
{
    public AuthMapper()
    {
        CreateMap<User, UserResponseDtoResponse>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));
    }
}
