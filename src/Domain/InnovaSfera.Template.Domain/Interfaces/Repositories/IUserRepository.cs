using InnovaSfera.Template.Domain.Entities;

namespace InnovaSfera.Template.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUserNameAsync(string userName);
    Task<User?> GetByIdAsync(Guid id);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> ExistsAsync(string email);
    Task<bool> ExistsByUserNameAsync(string userName);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
}
