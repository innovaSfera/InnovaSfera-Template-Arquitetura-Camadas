using AutoMapper;
using InnovaSfera.Template.Application.Dto.Request;
using InnovaSfera.Template.Application.Dto.Response;
using InnovaSfera.Template.Application.Interfaces;
using InnovaSfera.Template.Domain.Interfaces.Services;
using InnovaSfera.Template.Domain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InnovaSfera.Template.Application.Services;

public class AuthAppService : IAuthAppService
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthAppService> _logger;
    private readonly JwtSettings _jwtSettings;

    public AuthAppService(
        IAuthService authService,
        ITokenService tokenService,
        IMapper mapper,
        ILogger<AuthAppService> logger,
        IOptions<JwtSettings> jwtSettings)
    {
        _authService = authService;
        _tokenService = tokenService;
        _mapper = mapper;
        _logger = logger;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress = null)
    {
        try
        {
            var user = await _authService.AuthenticateAsync(request.Email, request.Password);
            
            if (user == null)
            {
                _logger.LogWarning("Login failed for email: {Email}", request.Email);
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Credenciais inválidas"
                };
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt for inactive user: {Email}", request.Email);
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Usuário inativo"
                };
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(ipAddress);

            // Atualizar o refresh token do usuário
            user.RefreshToken = refreshToken.Token;
            user.RefreshTokenExpiryTime = refreshToken.Expires;
            user.LastLoginAt = DateTime.UtcNow;

            await _authService.RefreshTokenAsync(refreshToken.Token, ipAddress);

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login realizado com sucesso",
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = _mapper.Map<UserResponseDto>(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return new AuthResponseDto
            {
                Success = false,
                Message = "Erro interno do servidor"
            };
        }
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            if (await _authService.EmailExistsAsync(request.Email))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email já está em uso"
                };
            }

            if (await _authService.UserNameExistsAsync(request.UserName))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Nome de usuário já está em uso"
                };
            }

            var user = await _authService.RegisterAsync(
                request.Email,
                request.UserName,
                request.Password,
                request.FirstName,
                request.LastName);

            _logger.LogInformation("User registered successfully: {Email}", request.Email);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Usuário registrado com sucesso",
                User = _mapper.Map<UserResponseDto>(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
            return new AuthResponseDto
            {
                Success = false,
                Message = "Erro interno do servidor"
            };
        }
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string? ipAddress = null)
    {
        try
        {
            var refreshToken = await _authService.RefreshTokenAsync(request.RefreshToken, ipAddress);
            
            // Buscar o usuário associado ao refresh token
            var userId = _tokenService.GetUserIdFromToken(request.RefreshToken);
            if (userId == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Token inválido"
                };
            }

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Usuário não encontrado"
                };
            }

            var accessToken = _tokenService.GenerateAccessToken(user);

            _logger.LogInformation("Token refreshed successfully for user: {UserId}", userId);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Token atualizado com sucesso",
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = _mapper.Map<UserResponseDto>(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return new AuthResponseDto
            {
                Success = false,
                Message = "Token inválido ou expirado"
            };
        }
    }

    public async Task<AuthResponseDto> RevokeTokenAsync(RefreshTokenRequestDto request, string? ipAddress = null)
    {
        try
        {
            var success = await _authService.RevokeTokenAsync(request.RefreshToken, ipAddress);
            
            if (!success)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Token inválido"
                };
            }

            _logger.LogInformation("Token revoked successfully");

            return new AuthResponseDto
            {
                Success = true,
                Message = "Token revogado com sucesso"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token revocation");
            return new AuthResponseDto
            {
                Success = false,
                Message = "Erro interno do servidor"
            };
        }
    }

    public async Task<UserResponseDto?> GetUserProfileAsync(Guid userId)
    {
        try
        {
            var user = await _authService.GetUserByIdAsync(userId);
            return user != null ? _mapper.Map<UserResponseDto>(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile for userId: {UserId}", userId);
            return null;
        }
    }
}
