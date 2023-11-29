using System.ComponentModel.DataAnnotations;
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
    public async Task<IActionResult> CreatePost(PostCreateEditDto createDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.ValidationState);
        
        var userGuid = (Guid)HttpContext.Items["UserId"]!;

        await postService.CreateUserPost(userGuid, createDto);
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
        var userGuid = (Guid?)HttpContext.Items["UserId"];
        
        var result = await postService.GetPostList(userGuid,
            tags, authorName, minReadingTime, maxReadingTime, sortingOption, onlyUserCommunities, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<PostFullDto>> GetFullPostInfo(Guid id)
    {
        var userGuid = (Guid?)HttpContext.Items["UserId"];
        
        var result = await postService.GetPostInfo(userGuid, id);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> EditPost(Guid id, PostCreateEditDto editDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.ValidationState);
        
        var userGuid = (Guid)HttpContext.Items["UserId"]!;

        await postService.EditPost(userGuid, id, editDto);
        return Ok();
    }

    [HttpPost("{postId}/like")]
    public async Task<IActionResult> LikePost(Guid postId)
    {
        var userGuid = (Guid)HttpContext.Items["UserId"]!;

        await postService.LikePost(userGuid, postId);
        return Ok();
    }

    [HttpDelete("{postId}/like")]
    public async Task<IActionResult> DeleteLike(Guid postId)
    {
        var userGuid = (Guid)HttpContext.Items["UserId"]!;

        await postService.RemoveLike(userGuid, postId);
        return Ok();
    }
}