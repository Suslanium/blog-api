using blog_api.Models;
using blog_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace blog_api.Controllers;

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
            if (e.Message == "Incorrect password")
            {
                return BadRequest("Incorrect email or password");
            }

            return StatusCode(500);
        }
        catch (InvalidOperationException)
        {
            return BadRequest("Incorrect email or password");
        }
    }
}