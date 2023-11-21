using System.Security.Claims;
using blog_api.Model;
using blog_api.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace blog_api.Controller;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState.ValidationState);
        }

        try
        {
            var token = await authService.Register(request);
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
            var token = await authService.Login(request);
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
        var email = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value;
        if (email == null)
        {
            return StatusCode(500);
        }

        await authService.InvalidateUserTokens(email);
        return Ok();
    }
}