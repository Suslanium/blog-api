using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using blog_api.Model;
using blog_api.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace blog_api.Controller;

[ApiController]
[Route("api/post")]
[Authorize]
public class PostController(IPostService postService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreatePost(CreatePostDto postDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.ValidationState);
        
        var identity = (HttpContext.User.Identity as ClaimsIdentity)!;
        var claims = identity.Claims;
        var guidString = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;

        await postService.CreateUserPost(Guid.Parse(guidString!), postDto);
        return Ok();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PostPagedListDto>> GetPosts(
        [FromQuery] List<Guid>? tags,
        [FromQuery] string? authorName,
        [FromQuery] int? minReadingTime,
        [FromQuery] int? maxReadingTime,
        [FromQuery] SortingOption? sortingOption,
        [FromQuery] [Required] bool onlyUserCommunities,
        [FromQuery] [Required] int pageNumber,
        [FromQuery] [Required] int pageSize
    )
    {
        var identity = (HttpContext.User.Identity as ClaimsIdentity)!;
        var claims = identity.Claims;
        var guidString = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;
        var result = await postService.GetPostList(guidString != null ? Guid.Parse(guidString) : null,
            tags, authorName, minReadingTime, maxReadingTime, sortingOption, onlyUserCommunities, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<PostFullDto>> GetFullPostInfo(Guid id)
    {
        var identity = (HttpContext.User.Identity as ClaimsIdentity)!;
        var claims = identity.Claims;
        var guidString = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;
        var result = await postService.GetPostInfo(guidString != null ? Guid.Parse(guidString) : null, id);
        return Ok(result);
    }

    [HttpPost("{postId}/like")]
    public async Task<IActionResult> LikePost(Guid postId)
    {
        var identity = (HttpContext.User.Identity as ClaimsIdentity)!;
        var claims = identity.Claims;
        var guidString = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;

        await postService.LikePost(Guid.Parse(guidString!), postId);
        return Ok();
    }

    [HttpDelete("{postId}/like")]
    public async Task<IActionResult> DeleteLike(Guid postId)
    {
        var identity = (HttpContext.User.Identity as ClaimsIdentity)!;
        var claims = identity.Claims;
        var guidString = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;

        await postService.RemoveLike(Guid.Parse(guidString!), postId);
        return Ok();
    }
}