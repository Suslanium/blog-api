using blog_api.Model;

namespace blog_api.Service;

public interface IUserService
{
    public Task<TokenResponse> Register(UserRegisterDto userRegisterDto);

    public Task<TokenResponse> Login(LoginCredentialsDto loginCredentials);

    public Task<UserDto> GetUserProfile(Guid guid);

    public Task Logout(Guid userGuid, DateTime tokenIssuedTime);

    public Task EditUserProfile(Guid guid, UserEditDto userEditDto);
}