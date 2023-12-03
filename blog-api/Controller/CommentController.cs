using System.Security.Claims;
using blog_api.Model;
using blog_api.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace blog_api.Controller;

[ApiController]
[Route("api/comment")]
[Authorize]
public class CommentController(ICommentService commentService) : ControllerBase
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
    
    [HttpPost("~/api/post/{id}/comment")]
    public async Task<IActionResult> AddComment(Guid id, CommentCreateDto createDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.ValidationState);

        await commentService.AddComment((Guid)UserId!, id, createDto);
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> EditComment(Guid id, CommentUpdateDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.ValidationState);

        await commentService.EditComment((Guid)UserId!, id, updateDto);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        await commentService.DeleteComment((Guid)UserId!, id);
        return Ok();
    }
}