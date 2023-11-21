using blog_api.Data.Models;
using blog_api.Model;

namespace blog_api.Repository;

public interface IUserRepository
{
    public Task<Boolean> UserExists(string email);

    public Task AddUser(User user);

    public Task<Boolean> CheckUserCredentials(LoginCredentialsDto loginCredentials);

    public Task InvalidateUserTokens(string email);
}