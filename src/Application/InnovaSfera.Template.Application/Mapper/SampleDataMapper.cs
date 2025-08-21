using AutoMapper;
using DomainDrivenDesign.Application.Entities;
using DomainDrivenDesign.Domain.Entities;

namespace DomainDrivenDesign.Application.Mapper;

public class SampleDataMapper : Profile
{
    public SampleDataMapper()
    {
        CreateMap<SampleDataDto, SampleData>().ReverseMap();
    }

}
