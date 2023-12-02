using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using blog_api.Data;
using blog_api.Data.Models;
using blog_api.Exception;
using blog_api.Model;
using blog_api.Model.Mapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace blog_api.Service;

public class UserService(BlogDbContext dbContext, IConfiguration configuration) : IUserService
{
    public async Task<string> Register(UserRegisterDto userRegisterDto)
    {
        if (await dbContext.Users.CountAsync(user => user.Email == userRegisterDto.Email) > 0)
            throw new BlogApiArgumentException("User with the same email already exists");

        var user = UserMapper.GetUserEntity(userRegisterDto);

        var userEntity = dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        return CreateToken(userEntity.Entity.Id);
    }

    public async Task<string> Login(LoginCredentialsDto loginCredentials)
    {
        var user = await dbContext.Users.Where(user => user.Email == loginCredentials.Email).FirstOrDefaultAsync();

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginCredentials.Password, user.PasswordHash))
            throw new BlogApiArgumentException("Incorrect email or password");

        return CreateToken(user.Id);
    }

    public async Task<UserDto> GetUserProfile(Guid id)
    {
        var userEntity = await dbContext.Users.FindAsync(id);
        if (userEntity == null)
            throw new BlogApiArgumentException("User does not exist");

        return UserMapper.GetUserDto(userEntity);
    }

    public async Task EditUserProfile(Guid guid, UserEditDto userEditDto)
    {
        var userEntity = await dbContext.Users.FindAsync(guid);
        if (userEntity == null)
            throw new BlogApiArgumentException("User does not exist");

        if (userEntity.Email != userEditDto.Email)
        {
            if (await dbContext.Users.CountAsync(user => user.Email == userEditDto.Email) > 0)
                throw new BlogApiArgumentException("User with the same email already exists");
        }

        userEntity.Email = userEditDto.Email;
        userEntity.FullName = userEditDto.FullName;
        userEntity.BirthDate = userEditDto.BirthDate;
        userEntity.Gender = userEditDto.Gender;
        userEntity.PhoneNumber = userEditDto.PhoneNumber;

        dbContext.Users.Update(userEntity);
        await dbContext.SaveChangesAsync();
    }
    
    
    public Task Logout(Guid userGuid, DateTime tokenIssuedTime)
    {
        var invalidationEntity = new InvalidTokenInfo
        {
            UserId = userGuid,
            IssuedTime = tokenIssuedTime
        };
        dbContext.InvalidatedTokens.Add(invalidationEntity);
        return dbContext.SaveChangesAsync();
    }

    private string CreateToken(Guid guid)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.GivenName, guid.ToString())
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