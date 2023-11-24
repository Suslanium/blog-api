using blog_api.Model;

namespace blog_api.Service;

public interface IUserService
{
    public Task<string> Register(UserRegisterDto userRegisterDto);

    public Task<string> Login(LoginCredentialsDto loginCredentials);

    public Task<UserDto> GetUserProfile(string email);

    public Task InvalidateUserTokens(string email);
}