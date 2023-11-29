using System.ComponentModel.DataAnnotations;
using blog_api.Data.Models;
using blog_api.Model;
using blog_api.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace blog_api.Controller;

[ApiController]
[Route("api/community")]
[Authorize]
public class CommunityController(ICommunityService communityService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<CommunityDto>>> GetCommunitiesList()
    {
        var result = await communityService.GetCommunityList();
        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<CommunityUserDto>>> GetUserCommunitiesList()
    {
        var userGuid = (Guid)HttpContext.Items["UserId"]!;

        var result = await communityService.GetUserCommunities(userGuid);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<CommunityFullDto>> GetCommunityDetails(Guid id)
    {
        var result = await communityService.GetCommunityDetails(id);
        return Ok(result);
    }

    [HttpGet("{id}/post")]
    [AllowAnonymous]
    public async Task<ActionResult<PostPagedListDto>> GetPosts(
        [FromQuery] List<Guid>? tags,
        [FromQuery] string? authorName,
        [FromQuery] int? minReadingTime,
        [FromQuery] int? maxReadingTime,
        [FromQuery] SortingOption? sortingOption,
        [FromQuery] [Required] int pageNumber,
        [FromQuery] [Required] int pageSize,
        Guid id
    )
    {
        var userGuid = (Guid?)HttpContext.Items["UserId"];

        var result = await communityService.GetCommunityPosts(userGuid, id,
            tags, authorName, minReadingTime, maxReadingTime, sortingOption, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpPost("{id}/post")]
    public async Task<IActionResult> CreatePost(Guid id, PostCreateEditDto editDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.ValidationState);

        var userGuid = (Guid)HttpContext.Items["UserId"]!;

        await communityService.CreatePost(userGuid, id, editDto);
        return Ok();
    }

    [HttpPost("{id}/subscribe")]
    public async Task<IActionResult> SubscribeToCommunity(Guid id)
    {
        var userGuid = (Guid)HttpContext.Items["UserId"]!;

        await communityService.SubscribeUser(userGuid, id);
        return Ok();
    }

    [HttpDelete("{id}/unsubscribe")]
    public async Task<IActionResult> UnsubscribeFromCommunity(Guid id)
    {
        var userGuid = (Guid)HttpContext.Items["UserId"]!;

        await communityService.UnsubscribeUser(userGuid, id);
        return Ok();
    }

    [HttpGet("{id}/role")]
    public async Task<ActionResult<CommunityRole>> GetUserRole(Guid id)
    {
        var userGuid = (Guid)HttpContext.Items["UserId"]!;

        var role = await communityService.GetUserRole(userGuid, id);
        return Ok(role);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateCommunity(CommunityCreateEditDto communityDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.ValidationState);

        var userGuid = (Guid)HttpContext.Items["UserId"]!;

        await communityService.CreateCommunity(userGuid, communityDto);
        return Ok();
    }

    [HttpPut("{id}/edit")]
    public async Task<IActionResult> EditCommunity(Guid id, CommunityCreateEditDto editDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.ValidationState);

        var userGuid = (Guid)HttpContext.Items["UserId"]!;

        await communityService.EditCommunity(userGuid, id, editDto);
        return Ok();
    }

    [HttpPost("{communityId}/admin/add/{userId}")]
    public async Task<IActionResult> AddAdministrator(Guid communityId, Guid userId)
    {
        var callerId = (Guid)HttpContext.Items["UserId"]!;

        await communityService.AddAdministrator(callerId, userId, communityId);
        return Ok();
    }

    [HttpDelete("{communityId}/admin/remove/{userId}")]
    public async Task<IActionResult> RemoveAdministrator(Guid communityId, Guid userId)
    {
        var callerId = (Guid)HttpContext.Items["UserId"]!;

        await communityService.RemoveAdministrator(callerId, userId, communityId);
        return Ok();
    }
}