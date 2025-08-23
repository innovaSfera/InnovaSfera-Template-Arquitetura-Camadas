using InnovaSfera.Template.Domain.Entities;
using InnovaSfera.Template.Domain.Interfaces.Repositories;
using InnovaSfera.Template.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace InnovaSfera.Template.Infrastructure.Data.Repositories;

/// <summary>
/// Implementação em memória do repositório de usuários para demonstração.
/// Em produção, substitua por implementação com Entity Framework ou outro ORM.
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users;
    private readonly ILogger<InMemoryUserRepository> _logger;
    private readonly IPasswordService _passwordService;

    public InMemoryUserRepository(ILogger<InMemoryUserRepository> logger, IPasswordService passwordService)
    {
        _logger = logger;
        _passwordService = passwordService;
        _users = new List<User>();
        
        // Seed com usuário administrador para testes
        SeedDefaultUsers();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        await Task.Delay(1); // Simular operação assíncrona
        return _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<User?> GetByUserNameAsync(string userName)
    {
        await Task.Delay(1); // Simular operação assíncrona
        return _users.FirstOrDefault(u => u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        await Task.Delay(1); // Simular operação assíncrona
        return _users.FirstOrDefault(u => u.Id == id);
    }

    public async Task<User> CreateAsync(User user)
    {
        await Task.Delay(1); // Simular operação assíncrona
        
        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;
        _users.Add(user);
        
        _logger.LogInformation("User created with ID: {UserId}", user.Id);
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        await Task.Delay(1); // Simular operação assíncrona
        
        var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existingUser == null)
        {
            throw new InvalidOperationException($"User with ID {user.Id} not found");
        }

        var index = _users.IndexOf(existingUser);
        _users[index] = user;
        
        _logger.LogInformation("User updated with ID: {UserId}", user.Id);
        return user;
    }

    public async Task<bool> ExistsAsync(string email)
    {
        await Task.Delay(1); // Simular operação assíncrona
        return _users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> ExistsByUserNameAsync(string userName)
    {
        await Task.Delay(1); // Simular operação assíncrona
        return _users.Any(u => u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        await Task.Delay(1); // Simular operação assíncrona
        return _users.FirstOrDefault(u => u.RefreshToken == refreshToken);
    }

    private void SeedDefaultUsers()
    {
        // Usuário administrador padrão para testes
        // Senha: Admin@123
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@innovasfera.com",
            UserName = "admin",
            PasswordHash = _passwordService.HashPassword("Admin@123"),
            FirstName = "Admin",
            LastName = "User",
            IsActive = true,
            IsEmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            Roles = new List<string> { "Admin", "User" }
        };

        _users.Add(adminUser);
        
        _logger.LogInformation("Default admin user seeded with email: {Email}", adminUser.Email);
    }
}
