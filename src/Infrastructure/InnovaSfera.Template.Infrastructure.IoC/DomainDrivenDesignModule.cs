using System.Reflection;
using DomainDrivenDesign.Application.Interfaces;
using DomainDrivenDesign.Application.Mapper;
using DomainDrivenDesign.Application.Services;
using DomainDrivenDesign.Domain.Interfaces;
using DomainDrivenDesign.Domain.Interfaces.Repositories;
using DomainDrivenDesign.Domain.Interfaces.Services;
using DomainDrivenDesign.Domain.Services;
using DomainDrivenDesign.Infrastructure.Data.Context;
using DomainDrivenDesign.Infrastructure.Data.Repositories;
using DomainDrivenDesign.Infrastructure.Data.UnitOfWork;
using FluentValidation;
using InnovaSfera.Template.Domain.ApiManager;
using InnovaSfera.Template.Domain.Interfaces.Cache;
using InnovaSfera.Template.Domain.Interfaces.External;
using InnovaSfera.Template.Domain.Interfaces.Storage;
using InnovaSfera.Template.Infrastructure.Data.Cache;
using InnovaSfera.Template.Infrastructure.Data.External;
using InnovaSfera.Template.Infrastructure.Data.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace DomainDrivenDesign.Infrastructure.IoC;

public static class DomainDrivenDesignModule
{
    public static void Register(this IServiceCollection services, IConfiguration configuration)
    {
        #region Redis
        RedisContext.Initialize(configuration);
        #endregion

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped<ISampleDataRepository, SampleDataRepository>();

        // Services
        services.AddScoped<ISampleDataService, SampleDataService>();

        // Mappers
        services.AddAutoMapper(typeof(SampleDataMapper));

        // App Services (Commands in future)
        services.AddScoped<ISampleDataAppService, SampleDataAppService>();

        #region Storage Strategy Pattern
        services.AddScoped<IStorageStrategyFactory, StorageStrategyFactory>();
        services.AddScoped<IStorageContext>(provider =>
        {
            var factory = provider.GetRequiredService<IStorageStrategyFactory>();
            var storageType = configuration["Storage:Type"] ?? "local";
            var strategy = factory.CreateStrategy(storageType);
            return new StorageContext(strategy);
        });
        #endregion

        #region Cache Strategy Pattern
        services.AddScoped<ICacheStrategyFactory, CacheStrategyFactory>();
        services.AddScoped<ICacheContext>(provider =>
        {
            var factory = provider.GetRequiredService<ICacheStrategyFactory>();
            var cacheType = configuration["Cache:Type"] ?? "memory";
            var strategy = factory.CreateStrategy(cacheType);
            return new CacheContext(strategy);
        });
        #endregion

        #region External API Managers
        // Configure Harry Potter API Options
        services.Configure<ApiOptionsManager>(
            configuration.GetSection("HarryPotterApi"));

        // Configure HttpClient with Polly policies for resilience
        services.AddHttpClient<IHarryPotterApiManager, HarryPotterApiManager>((sp, http) =>
        {
            var options = sp.GetRequiredService<IOptions<ApiOptionsManager>>().Value;
            http.BaseAddress = new Uri(options.BaseUrl);
            http.Timeout = options.Timeout;
            http.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        })
        // Retry policy with exponential backoff
        .AddPolicyHandler(HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => (int)r.StatusCode == 429) // Handle rate limiting
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromMilliseconds(200),
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromSeconds(1)
            }))
        // Circuit breaker policy
        .AddPolicyHandler(HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
        #endregion

        #region Fluent
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        #endregion

        #region Redis
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["REDIS_CONNECTION_STRING"] ?? "localhost:6379";
        });
        #endregion
    }

    public static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SampleContext>(opt => opt.UseInMemoryDatabase("SampleData"));
    }
}
