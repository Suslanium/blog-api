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

    [HttpPost("register")]
    public async Task<ActionResult<TokenResponse>> Register(UserRegisterDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.ValidationState);

        var token = await userService.Register(request);
        return Ok(token);
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login(LoginCredentialsDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.ValidationState);

        var token = await userService.Login(request);
        return Ok(token);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var iatClaim = HttpContext.User.FindFirst("nbf")!;
        var issuedAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(iatClaim.Value)).UtcDateTime;

        await userService.Logout((Guid)UserId!, issuedAt);
        return Ok();
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetUserProfile()
    {
        var result = await userService.GetUserProfile((Guid)UserId!);
        return Ok(result);
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> EditUserProfile(UserEditDto userEditDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.ValidationState);

        await userService.EditUserProfile((Guid)UserId!, userEditDto);
        return Ok();
    }
}