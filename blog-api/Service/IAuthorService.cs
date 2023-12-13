using blog_api.Model;

namespace blog_api.Service;

public interface IAuthorService
{
    public Task<List<AuthorDto>> GetAuthorList();
}