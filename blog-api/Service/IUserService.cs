using blog_api.Model;

namespace blog_api.Service;

public interface IUserService
{
    public Task<string> Register(UserRegisterDto userRegisterDto);

    public Task<string> Login(LoginCredentialsDto loginCredentials);

    public Task<UserDto> GetUserProfile(Guid guid);

    public Task EditUserProfile(Guid guid, UserEditDto userEditDto);

    public Task InvalidateUserTokens(Guid guid);
}