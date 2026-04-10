using FirstApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using FirstApi.Data;
using FirstApi.DTOs;
using FirstApi.Models;
namespace FirstApi.Repositories;

public class AuthRepository(FirstApiContext context) : IAuthRepository
{
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task AddUserAsync(User user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
       return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task UpdateUserAsync(User user)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }

    public async Task<User?> GetUserByEmailAndTokenAsync(string email, string token)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email && u.EmailVerificationToken == token);
    }


    public async Task<RefreshToken?> GetRefreshTokenEntityAsync(string refreshToken)
    {
        return await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
    }

    public async Task SaveRefreshTokenAsync(RefreshToken refreshToken)
    {
        await context.RefreshTokens.AddAsync(refreshToken);
        await context.SaveChangesAsync();
    }

    public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
    {
        context.RefreshTokens.Update(refreshToken);
        await context.SaveChangesAsync();
    }

    public async Task RevokeTokenFamilyAsync(string familyId)
    {
        var tokens = await context.RefreshTokens.Where(rt => rt.FamilyId == familyId).ToListAsync();
        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }
        await context.SaveChangesAsync();
    }
}