using blog_api.Data;
using blog_api.Data.Models;
using blog_api.Model;
using Microsoft.EntityFrameworkCore;

namespace blog_api.Repository;

public class UserRepository(BlogDbContext dbContext) : IUserRepository
{
    public async Task<bool> UserExists(string email)
    {
        return await dbContext.Users.CountAsync(user => user.Email == email) > 0;
    }

    public async Task AddUser(User user)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> CheckUserCredentials(LoginCredentialsDto loginCredentials)
    {
        var user = await dbContext.Users.Where(user => user.Email == loginCredentials.Email).FirstOrDefaultAsync();

        return !(user == null || !BCrypt.Net.BCrypt.Verify(loginCredentials.Password, user.PasswordHash));
    }
}