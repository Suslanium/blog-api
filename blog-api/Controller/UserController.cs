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

        try
        {
            var token = await userService.Register(request);
            return Ok(token);
        }
        catch (ArgumentException e)
        {
            if (e.Message == "User with the same email already exists")
            {
                return BadRequest("User with the same email already exists");
            }

            return StatusCode(500);
        }
        catch (InvalidOperationException)
        {
            return StatusCode(500);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCredentialsDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState.ValidationState);
        }

        try
        {
            var token = await userService.Login(request);
            return Ok(token);
        }
        catch (ArgumentException e)
        {
            if (e.Message == "Incorrect email or password")
            {
                return BadRequest("Incorrect email or password");
            }

            return StatusCode(500);
        }
        catch (InvalidOperationException)
        {
            return StatusCode(500);
        }
    }

    [HttpPost("logoutAll")]
    [Authorize]
    public async Task<IActionResult> LogoutAll()
    {
        if (HttpContext.User.Identity is not ClaimsIdentity identity) return StatusCode(500);
        var claims = identity.Claims;
        var guidString = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;
        if (guidString == null)
        {
            return StatusCode(500);
        }

        await userService.InvalidateUserTokens(Guid.Parse(guidString));
        return Ok();
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetUserProfile()
    {
        if (HttpContext.User.Identity is not ClaimsIdentity identity) return StatusCode(500);
        var claims = identity.Claims;
        var guidString = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;
        if (guidString == null)
        {
            return StatusCode(500);
        }

        var result = await userService.GetUserProfile(Guid.Parse(guidString));
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
        
        if (HttpContext.User.Identity is not ClaimsIdentity identity) return StatusCode(500);
        var claims = identity.Claims;
        var guidString = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;
        if (guidString == null)
        {
            return StatusCode(500);
        }

        await userService.EditUserProfile(Guid.Parse(guidString), userEditDto);
        return Ok();
    }
}