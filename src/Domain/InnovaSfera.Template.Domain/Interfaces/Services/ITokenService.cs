using InnovaSfera.Template.Domain.Entities;

namespace InnovaSfera.Template.Domain.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(string? ipAddress = null);
    bool ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
    string? GetClaimFromToken(string token, string claimType);
}
