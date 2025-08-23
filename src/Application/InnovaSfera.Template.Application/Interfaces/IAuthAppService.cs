using InnovaSfera.Template.Application.Dto.Request;
using InnovaSfera.Template.Application.Dto.Response;

namespace InnovaSfera.Template.Application.Interfaces;

public interface IAuthAppService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress = null);
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string? ipAddress = null);
    Task<AuthResponseDto> RevokeTokenAsync(RefreshTokenRequestDto request, string? ipAddress = null);
    Task<UserResponseDto?> GetUserProfileAsync(Guid userId);
}
