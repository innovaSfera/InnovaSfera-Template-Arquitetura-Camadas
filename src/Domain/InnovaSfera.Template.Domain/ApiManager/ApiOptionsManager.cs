namespace InnovaSfera.Template.Domain.ApiManager;

public class ApiOptionsManager
{
    public string BaseUrl { get; set; } = "";
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
    public int RetryCount { get; set; } = 3;
    public int CircuitBreakerFailureThreshold { get; set; } = 5;
    public TimeSpan CircuitBreakerDuration { get; set; } = TimeSpan.FromSeconds(30);
}
