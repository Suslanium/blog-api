using blog_api.Models;

namespace blog_api.Services;

public interface IAuthService
{
    public Task<string> Register(UserDto userDto);

    public Task<string> Login(LoginCredentialsDto loginCredentials);
}