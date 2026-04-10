using FirstApi.DTOs;
using FirstApi.Models;

namespace FirstApi.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<bool> ExistsByEmailAsync(string email);
    Task AddUserAsync(User user);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAndTokenAsync(string email, string token);
    Task UpdateUserAsync(User user);

    Task<RefreshToken?> GetRefreshTokenEntityAsync(string refreshToken);
    Task SaveRefreshTokenAsync(RefreshToken refreshToken);
    Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
    Task RevokeTokenFamilyAsync(string familyId);

}