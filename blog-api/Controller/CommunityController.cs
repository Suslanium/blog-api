using System.Security.Claims;
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
        var identity = (HttpContext.User.Identity as ClaimsIdentity)!;
        var claims = identity.Claims;
        var guidString = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;

        var result = await communityService.GetUserCommunities(Guid.Parse(guidString!));
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<CommunityFullDto>> GetCommunityDetails(Guid id)
    {
        var result = await communityService.GetCommunityDetails(id);
        return Ok(result);
    }
    
    [HttpPost("{id}/subscribe")]
    public async Task<IActionResult> SubscribeToCommunity(Guid id)
    {
        var identity = (HttpContext.User.Identity as ClaimsIdentity)!;
        var claims = identity.Claims;
        var guidString = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;

        await communityService.SubscribeUser(Guid.Parse(guidString!), id);
        return Ok();
    }

    [HttpDelete("{id}/unsubscribe")]
    public async Task<IActionResult> UnsubscribeFromCommunity(Guid id)
    {
        var identity = (HttpContext.User.Identity as ClaimsIdentity)!;
        var claims = identity.Claims;
        var guidString = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;

        await communityService.UnsubscribeUser(Guid.Parse(guidString!), id);
        return Ok();
    }
    
    [HttpGet("{id}/role")]
    public async Task<ActionResult<CommunityRole>> GetUserRole(Guid id)
    {
        var identity = (HttpContext.User.Identity as ClaimsIdentity)!;
        var claims = identity.Claims;
        var guidString = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;

        var role = await communityService.GetUserRole(Guid.Parse(guidString!), id);
        return Ok(role);
    }
    
    [HttpPost("create")]
    public async Task<IActionResult> CreateCommunity(CommunityCreateEditDto communityDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.ValidationState);

        var identity = (HttpContext.User.Identity as ClaimsIdentity)!;
        var claims = identity.Claims;
        var guidString = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;

        await communityService.CreateCommunity(Guid.Parse(guidString!), communityDto);
        return Ok();
    }

    [HttpPost("{communityId}/admin/add/{userId}")]
    public async Task<IActionResult> AddAdministrator(Guid communityId, Guid userId)
    {
        var identity = (HttpContext.User.Identity as ClaimsIdentity)!;
        var claims = identity.Claims;
        var guidString = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;

        await communityService.AddAdministrator(Guid.Parse(guidString!), userId, communityId);
        return Ok();
    }

    [HttpDelete("{communityId}/admin/remove/{userId}")]
    public async Task<IActionResult> RemoveAdministrator(Guid communityId, Guid userId)
    {
        var identity = (HttpContext.User.Identity as ClaimsIdentity)!;
        var claims = identity.Claims;
        var guidString = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;

        await communityService.RemoveAdministrator(Guid.Parse(guidString!), userId, communityId);
        return Ok();
    }
}