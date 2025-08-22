using AutoMapper;
using DomainDrivenDesign.Application.Entities;
using DomainDrivenDesign.Domain.Entities;
using InnovaSfera.Template.Application.Dto.Response;
using InnovaSfera.Template.Domain.Entities;

namespace DomainDrivenDesign.Application.Mapper;

public class SampleDataMapper : Profile
{
    public SampleDataMapper()
    {
        CreateMap<SampleDataDto, SampleData>().ReverseMap();
        
        // Character mappings
        CreateMap<Character, CharacterDtoResponse>().ReverseMap();
        CreateMap<Wand, WandDtoResponse>().ReverseMap();
    }
}
