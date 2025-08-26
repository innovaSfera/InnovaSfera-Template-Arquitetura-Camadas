namespace InnovaSfera.Template.Application.Dto.Response;

public class AuthResponseDtoResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserResponseDtoResponse? User { get; set; }
}
