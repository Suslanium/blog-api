using blog_api.Model;

namespace blog_api.Service;

public interface IAuthService
{
    public Task<string> Register(UserDto userDto);

    public Task<string> Login(LoginCredentialsDto loginCredentials);
}