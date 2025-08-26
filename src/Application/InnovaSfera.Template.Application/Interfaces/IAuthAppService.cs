using InnovaSfera.Template.Application.Dto.Request;
using InnovaSfera.Template.Application.Dto.Response;

namespace InnovaSfera.Template.Application.Interfaces;

public interface IAuthAppService
{
    Task<AuthResponseDtoResponse> LoginAsync(LoginRequestDto request, string? ipAddress = null);
    Task<AuthResponseDtoResponse> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDtoResponse> RefreshTokenAsync(RefreshTokenRequestDto request, string? ipAddress = null);
    Task<AuthResponseDtoResponse> RevokeTokenAsync(RefreshTokenRequestDto request, string? ipAddress = null);
    Task<UserResponseDtoResponse?> GetUserProfileAsync(Guid userId);
}
