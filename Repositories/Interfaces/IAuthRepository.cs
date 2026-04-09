using FirstApi.Models;

namespace FirstApi.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<bool> ExistsByEmailAsync(string email);
    Task AddUserAsync(User user);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByEmailAndTokenAsync(string email, string token);
    Task UpdateUserAsync(User user);

    Task<User?> GetUserByRefreshTokenAsync(string refreshToken);

}