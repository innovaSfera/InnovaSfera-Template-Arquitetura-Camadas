using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using InnovaSfera.Template.Domain.Entities;
using InnovaSfera.Template.Domain.Interfaces.External;
using InnovaSfera.Template.Infrastructure.Data.Configurations.ApiManagerConfig;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InnovaSfera.Template.Infrastructure.Data.External;

public class HarryPotterApiManager : ApiBase, IHarryPotterApiManager
{
    private readonly ILogger<HarryPotterApiManager> _logger;
    private readonly IConfiguration _configuration;

    public HarryPotterApiManager(
        ILogger<HarryPotterApiManager> logger, 
        IConfiguration configuration, 
        HttpClient httpClient) 
        : base(httpClient.BaseAddress?.ToString()!, httpClient, logger)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<ICollection<Character>> WizardsGetAllAsync()
    {
        try
        {
            _logger.LogInformation("Starting request to get all wizards");

            using var req = new HttpRequestMessage(HttpMethod.Get, "api/characters");
            req.Headers.Accept.ParseAdd("application/json");

            using var resp = await ApiClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, CancellationToken);

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Characters not found (404)");
                return Array.Empty<Character>();
            }

            resp.EnsureSuccessStatusCode();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = await resp.Content.ReadFromJsonAsync<List<Character>>(options, CancellationToken);

            _logger.LogInformation("Successfully retrieved {Count} characters", data?.Count ?? 0);

            return data ?? new List<Character>();
        }
        catch (Exception ex)
        {
            await LogApiExceptionAsync(ex);
            throw;
        }
    }
}
