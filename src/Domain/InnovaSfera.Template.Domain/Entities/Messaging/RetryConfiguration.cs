namespace InnovaSfera.Template.Domain.Entities.Messaging;

public class RetryConfiguration
{
    public int MaxRetries { get; set; } = 3;
    public int BaseDelayMs { get; set; } = 1000;
    public int MaxDelayMs { get; set; } = 30000;
    public bool UseExponentialBackoff { get; set; } = true;
    public bool SendToDlqOnMaxRetries { get; set; } = true;

    public static RetryConfiguration Default => new();
}
