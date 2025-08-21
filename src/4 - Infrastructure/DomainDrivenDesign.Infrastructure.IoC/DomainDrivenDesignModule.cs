using DomainDrivenDesign.Application.Interfaces;
using DomainDrivenDesign.Application.Mapper;
using DomainDrivenDesign.Application.Services;
using DomainDrivenDesign.Domain.Interfaces.Repositories;
using DomainDrivenDesign.Domain.Interfaces.Services;
using DomainDrivenDesign.Domain.Services;
using DomainDrivenDesign.Infrastructure.Data.Context;
using DomainDrivenDesign.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DomainDrivenDesign.Infrastructure.IoC;

public static class DomainDrivenDesignModule
{
    public static void Register(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<ISampleDataRepository, SampleDataRepository>();

        // Services
        services.AddScoped<ISampleDataService, SampleDataService>();

        // Mappers
        services.AddAutoMapper(typeof(SampleDataMapper));
        
        // App Services (Commands in future)
        services.AddScoped<ISampleDataAppService, SampleDataAppService>();
    }

    public static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SampleContext>(opt => opt.UseInMemoryDatabase("SampleData"));
    }
}
