using InnovaSfera.Template.Domain.Entities;
using InnovaSfera.Template.Domain.Interfaces.Repositories;
using InnovaSfera.Template.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace InnovaSfera.Template.Infrastructure.Data.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            
            if (user == null)
            {
                _logger.LogWarning("Authentication failed: User not found for email {Email}", email);
                return null;
            }

            if (!_passwordService.VerifyPassword(password, user.PasswordHash))
            {
                _logger.LogWarning("Authentication failed: Invalid password for email {Email}", email);
                return null;
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Authentication failed: User is inactive for email {Email}", email);
                return null;
            }

            _logger.LogInformation("User authenticated successfully: {Email}", email);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for email: {Email}", email);
            return null;
        }
    }

    public async Task<User> RegisterAsync(string email, string userName, string password, string? firstName = null, string? lastName = null)
    {
        try
        {
            if (await _userRepository.ExistsAsync(email))
            {
                throw new InvalidOperationException("Email já está em uso");
            }

            if (await _userRepository.ExistsByUserNameAsync(userName))
            {
                throw new InvalidOperationException("Nome de usuário já está em uso");
            }

            if (!_passwordService.IsPasswordStrong(password))
            {
                throw new InvalidOperationException("Senha não atende aos critérios de segurança");
            }

            var user = new User
            {
                Email = email.ToLowerInvariant(),
                UserName = userName,
                PasswordHash = _passwordService.HashPassword(password),
                FirstName = firstName,
                LastName = lastName,
                IsActive = true,
                IsEmailConfirmed = false,
                CreatedAt = DateTime.UtcNow,
                Roles = new List<string> { "User" }
            };

            var createdUser = await _userRepository.CreateAsync(user);
            
            _logger.LogInformation("User registered successfully: {Email}", email);
            return createdUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for email: {Email}", email);
            throw;
        }
    }

    public async Task<RefreshToken> RefreshTokenAsync(string token, string? ipAddress = null)
    {
        try
        {
            var user = await _userRepository.GetByRefreshTokenAsync(token);
            
            if (user == null)
            {
                throw new SecurityTokenException("Token inválido");
            }

            // Verificar se o refresh token ainda é válido
            if (user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new SecurityTokenException("Token expirado");
            }

            // Gerar novo refresh token
            var newRefreshToken = _tokenService.GenerateRefreshToken(ipAddress);
            
            // Atualizar o usuário com o novo refresh token
            user.RefreshToken = newRefreshToken.Token;
            user.RefreshTokenExpiryTime = newRefreshToken.Expires;
            
            await _userRepository.UpdateAsync(user);
            
            _logger.LogInformation("Refresh token updated for user: {UserId}", user.Id);
            return newRefreshToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            throw;
        }
    }

    public async Task<bool> RevokeTokenAsync(string token, string? ipAddress = null)
    {
        try
        {
            var user = await _userRepository.GetByRefreshTokenAsync(token);
            
            if (user == null)
            {
                _logger.LogWarning("Attempt to revoke invalid token");
                return false;
            }

            // Remover o refresh token
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            
            await _userRepository.UpdateAsync(user);
            
            _logger.LogInformation("Token revoked for user: {UserId}", user.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token revocation");
            return false;
        }
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            return await _userRepository.GetByIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        try
        {
            return await _userRepository.ExistsAsync(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if email exists: {Email}", email);
            return false;
        }
    }

    public async Task<bool> UserNameExistsAsync(string userName)
    {
        try
        {
            return await _userRepository.ExistsByUserNameAsync(userName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if username exists: {UserName}", userName);
            return false;
        }
    }
}
