using InnovaSfera.Template.Domain.Entities;

namespace InnovaSfera.Template.Domain.Interfaces.Services;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(string email, string password);
    Task<User> RegisterAsync(string email, string userName, string password, string? firstName = null, string? lastName = null);
    Task<RefreshToken> RefreshTokenAsync(string token, string? ipAddress = null);
    Task<bool> RevokeTokenAsync(string token, string? ipAddress = null);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UserNameExistsAsync(string userName);
}
