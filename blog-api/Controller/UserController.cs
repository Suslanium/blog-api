using System.Security.Claims;
using blog_api.Model;
using blog_api.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace blog_api.Controller;

[ApiController]
[Route("api/account")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState.ValidationState);
        }

        var token = await userService.Register(request);
        return Ok(token);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCredentialsDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState.ValidationState);
        }

        var token = await userService.Login(request);
        return Ok(token);
    }

    [HttpPost("logoutAll")]
    [Authorize]
    public async Task<IActionResult> LogoutAll()
    {
        var identity = (HttpContext.User.Identity as ClaimsIdentity)!;
        var claims = identity.Claims;
        var guidString = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;

        await userService.InvalidateUserTokens(Guid.Parse(guidString!));
        return Ok();
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetUserProfile()
    {
        var identity = (HttpContext.User.Identity as ClaimsIdentity)!;
        var claims = identity.Claims;
        var guidString = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;

        var result = await userService.GetUserProfile(Guid.Parse(guidString!));
        return Ok(result);
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> EditUserProfile(UserEditDto userEditDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState.ValidationState);
        }

        var identity = (HttpContext.User.Identity as ClaimsIdentity)!;
        var claims = identity.Claims;
        var guidString = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;

        await userService.EditUserProfile(Guid.Parse(guidString!), userEditDto);
        return Ok();
    }
}