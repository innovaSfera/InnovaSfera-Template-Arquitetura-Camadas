using Microsoft.Extensions.Logging;

namespace InnovaSfera.Template.Infrastructure.Data.Configurations.ApiManagerConfig;

public class ApiBase
{

    public readonly HttpClient ApiClient;
    public readonly ILogger Logger;
    private CancellationToken _cancellationToken;
    public CancellationToken CancellationToken => _cancellationToken;

    public ApiBase(string path, HttpClient apiClient, ILogger logger)
    {
        ApiClient = apiClient;
        Logger = logger;
        ArgumentNullException.ThrowIfNull(path, "path");
        ApiClient.BaseAddress = new Uri(path);
    }

    public void SetCancellationToken(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    public async Task<string> LogApiExceptionAsync(Exception exception)
    {
        string text = exception.Message;
        Logger.LogError(exception, "An error occurred while calling the API: {Message}", text);
        return text;
    }

}
