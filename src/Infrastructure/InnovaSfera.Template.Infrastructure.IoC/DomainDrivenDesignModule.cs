using System.Reflection;
using DomainDrivenDesign.Application.Interfaces;
using DomainDrivenDesign.Application.Mapper;
using DomainDrivenDesign.Application.Services;
using DomainDrivenDesign.Domain.Interfaces;
using DomainDrivenDesign.Domain.Interfaces.Messaging;
using DomainDrivenDesign.Domain.Services.Messaging;
using DomainDrivenDesign.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using DomainDrivenDesign.Domain.Interfaces.Services;
using DomainDrivenDesign.Domain.Services;
using DomainDrivenDesign.Infrastructure.Data.Context;
using DomainDrivenDesign.Infrastructure.Data.Repositories;
using DomainDrivenDesign.Infrastructure.Data.UnitOfWork;
using FluentValidation;
using InnovaSfera.Template.Application.Interfaces;
using InnovaSfera.Template.Application.Mapper;
using InnovaSfera.Template.Application.Services;
using InnovaSfera.Template.Domain.ApiManager;
using InnovaSfera.Template.Domain.Interfaces.Cache;
using InnovaSfera.Template.Domain.Interfaces.External;
using InnovaSfera.Template.Domain.Interfaces.Repositories;
using InnovaSfera.Template.Domain.Interfaces.Services;
using InnovaSfera.Template.Domain.Interfaces.Storage;
using InnovaSfera.Template.Domain.Settings;
using InnovaSfera.Template.Infrastructure.Data.Cache;
using InnovaSfera.Template.Infrastructure.Data.External;
using InnovaSfera.Template.Infrastructure.Data.Messaging.Adapters;
using InnovaSfera.Template.Infrastructure.Data.Repositories;
using InnovaSfera.Template.Infrastructure.Data.Services;
using InnovaSfera.Template.Infrastructure.Data.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Extensions.Http;
using System.Text;

namespace DomainDrivenDesign.Infrastructure.IoC;

public static class DomainDrivenDesignModule
{
    public static void Register(this IServiceCollection services, IConfiguration configuration)
    {
        #region Database Configuration - Choose ONE

        // === ENTITY FRAMEWORK CONFIGURATION === (ACTIVE)
        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        // Repositories
        services.AddScoped<ISampleDataRepository, SampleDataRepository>();

        // === DAPPER CONFIGURATION === (COMMENTED - Uncomment to use)
        // To use Dapper instead of Entity Framework:
        // 1. Comment the Entity Framework lines above
        // 2. Uncomment the Dapper lines below
        // 3. Configure connection string in appsettings.json
        //
        // services.AddScoped<SampleContextDapper>();
        // services.AddScoped<IUnitOfWork, UnitOfWorkDapper>();
        // services.AddScoped<ISampleDataRepository, SampleDataRepositoryDapper>();

        #endregion

        // Common repositories (independent of ORM choice)
        services.AddScoped<IUserRepository, InMemoryUserRepository>();

        // Services
        services.AddScoped<ISampleDataService, SampleDataService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();

        // Mappers
        services.AddAutoMapper(typeof(SampleDataMapper));
        services.AddAutoMapper(typeof(AuthMapper));

        // App Services (Commands in future)
        services.AddScoped<ISampleDataAppService, SampleDataAppService>();
        services.AddScoped<IAuthAppService, AuthAppService>();
        services.AddScoped<IMessagingAppService, MessagingApplicationService>();
        services.AddScoped<IStorageAppService, StorageAppService>();

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
        RedisContext.Initialize(configuration);
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["REDIS_CONNECTION_STRING"] ?? "localhost:6379";
        });
        #endregion

        #region JWT Authentication
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
        if (jwtSettings != null && !string.IsNullOrEmpty(jwtSettings.SecretKey))
        {
            var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = jwtSettings.SaveToken;
                options.RequireHttpsMetadata = jwtSettings.RequireHttpsMetadata;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = jwtSettings.ValidateIssuer,
                    ValidateAudience = jwtSettings.ValidateAudience,
                    ValidateLifetime = jwtSettings.ValidateLifetime,
                    ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.FromSeconds(jwtSettings.ClockSkewSeconds)
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Suporte para refresh token via cookie
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/api"))
                        {
                            context.Token = accessToken;
                        }
                        
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers["Token-Expired"] = "true";
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization();
        }
        #endregion

        #region Messaging Configuration - Choose ONE provider

        // Core messaging service with Polly patterns
        services.AddScoped<IMessagingService, MessagingService>();

        // === MESSAGING ADAPTERS === (Choose one or multiple)
        // Comment/Uncomment based on your needs and install the appropriate NuGet packages

        // RABBITMQ ADAPTER (Install: RabbitMQ.Client)
        // services.AddScoped<IMessageAdapter, RabbitMqMessageSender>();

        // KAFKA ADAPTER (Install: Confluent.Kafka)
        // services.AddScoped<IMessageAdapter, KafkaMessageSender>();

        // AZURE SERVICE BUS ADAPTER (Install: Azure.Messaging.ServiceBus)
        // services.AddScoped<IMessageAdapter, AzureServiceBusMessageSender>();

        // AWS SQS ADAPTER (Install: AWSSDK.SQS)
        // services.AddScoped<IMessageAdapter, SqsMessageSender>();

        // === DEFAULT CONFIGURATION ===
        // For development/testing - uses no-op implementation
        // Remove or comment this when configuring a real provider above
        services.AddScoped<IMessageAdapter, NoOpMessageSender>();

        #endregion
    }

    public static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        #region Database Context Configuration - Choose ONE

        // === ENTITY FRAMEWORK CONFIGURATION === (ACTIVE)
        // For Development - In Memory Database
        services.AddDbContext<SampleContext>(opt => opt.UseInMemoryDatabase("SampleData"));
        
        // For Production - SQL Server Database (uncomment and configure connection string)
        // services.AddDbContext<SampleContext>(opt =>
        // {
        //     opt.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        // });

        // === DAPPER CONFIGURATION === (COMMENTED - Uncomment to use)
        // When using Dapper, you don't need AddDbContext for Entity Framework
        // The SampleContextDapper is registered directly in the Register method above
        // Just ensure you have the connection string configured:
        //
        // "ConnectionStrings": {
        //   "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=InnovaSferaTemplate;Trusted_Connection=true;MultipleActiveResultSets=true"
        // }

        #endregion
    }
}
