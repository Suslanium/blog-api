using blog_api.Model;
using blog_api.Service;
using Microsoft.AspNetCore.Mvc;

namespace blog_api.Controller;

[ApiController]
[Route("api/author")]
public class AuthorController(IAuthorService authorService) : ControllerBase
{
    [HttpGet("list")]
    public async Task<ActionResult<List<AuthorDto>>> GetAuthorList()
    {
        var result = await authorService.GetAuthorList();
        return Ok(result);
    }
}