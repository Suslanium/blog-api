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
    private Guid? UserId
    {
        get
        {
            var identity = (HttpContext.User.Identity as ClaimsIdentity)!;
            var claims = identity.Claims;
            var guidString = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;
            return guidString == null ? null : Guid.Parse(guidString);
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> CreatePost(PostCreateEditDto createDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.ValidationState);

        await postService.CreateUserPost((Guid)UserId!, createDto);
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
        var result = await postService.GetPostList(UserId,
            tags, authorName, minReadingTime, maxReadingTime, sortingOption, onlyUserCommunities, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<PostFullDto>> GetFullPostInfo(Guid id)
    {
        var result = await postService.GetPostInfo(UserId, id);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> EditPost(Guid id, PostCreateEditDto editDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.ValidationState);

        await postService.EditPost((Guid)UserId!, id, editDto);
        return Ok();
    }

    [HttpPost("{postId}/like")]
    public async Task<IActionResult> LikePost(Guid postId)
    {
        await postService.LikePost((Guid)UserId!, postId);
        return Ok();
    }

    [HttpDelete("{postId}/like")]
    public async Task<IActionResult> DeleteLike(Guid postId)
    {
        await postService.RemoveLike((Guid)UserId!, postId);
        return Ok();
    }
}