using FirstApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using FirstApi.Data;
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

    public async Task UpdateUserAsync(User user)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }

    public async Task<User?> GetUserByEmailAndTokenAsync(string email, string token)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email && u.EmailVerificationToken == token);
    }
}