using blog_api.Data;
using blog_api.Model;
using blog_api.Model.Mapper;
using Microsoft.EntityFrameworkCore;

namespace blog_api.Service.Impl;

public class AuthorService(BlogDbContext dbContext) : IAuthorService
{
    public Task<List<AuthorDto>> GetAuthorList()
    {
        return dbContext.Users.Where(user => user.Posts.Count > 0).OrderBy(user => user.FullName)
            .Select(UserMapper.ConvertToAuthorDto()).ToListAsync();
    }
}