using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using blog_api.Data.Models;
using blog_api.Model;
using blog_api.Repository;
using Microsoft.IdentityModel.Tokens;

namespace blog_api.Service;

public class AuthService(IUserRepository userRepository, IConfiguration configuration) : IAuthService
{
    public async Task<string> Register(UserDto userDto)
    {
        if (await userRepository.UserExists(userDto.Email))
            throw new ArgumentException("User with the same email already exists");

        var user = new User
        {
            FullName = userDto.FullName,
            Email = userDto.Email,
            Gender = userDto.Gender,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
            PhoneNumber = userDto.PhoneNumber
        };

        await userRepository.AddUser(user);

        return CreateToken(userDto.Email);
    }

    public async Task<string> Login(LoginCredentialsDto loginCredentials)
    {
        if (!await userRepository.CheckUserCredentials(loginCredentials))
            throw new ArgumentException("Incorrect email or password");

        return CreateToken(loginCredentials.Email);
    }

    public async Task InvalidateUserTokens(string email)
    {
        await userRepository.InvalidateUserTokens(email);
    }

    private string CreateToken(string email)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration.GetSection("AppSettings:Token").Value!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}