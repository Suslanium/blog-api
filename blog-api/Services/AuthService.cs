using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using blog_api.Data;
using blog_api.Data.Models;
using blog_api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace blog_api.Services;

public class AuthService(BlogDbContext dbContext, IConfiguration configuration) : IAuthService
{
    public async Task<string> Register(UserDto userDto)
    {
        if (await dbContext.Users.CountAsync(user => user.Email == userDto.Email) > 0)
        {
            throw new ArgumentException("User with the same email already exists");
        }

        var user = new User
        {
            FullName = userDto.FullName,
            Email = userDto.Email,
            Gender = userDto.Gender,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
            PhoneNumber = userDto.PhoneNumber
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return CreateToken(userDto.Email);
    }

    public async Task<string> Login(LoginCredentialsDto loginCredentials)
    {
        var user = await dbContext.Users.Where(user => user.Email == loginCredentials.Email).FirstAsync();

        if (!BCrypt.Net.BCrypt.Verify(loginCredentials.Password, user.PasswordHash))
        {
            throw new ArgumentException("Incorrect password");
        }

        return CreateToken(loginCredentials.Email);
    }

    private string CreateToken(string email)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration.GetSection("AppSettings:Token").Value!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}