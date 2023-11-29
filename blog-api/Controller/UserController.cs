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
            return BadRequest(ModelState.ValidationState);

        var token = await userService.Register(request);
        return Ok(token);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCredentialsDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.ValidationState);

        var token = await userService.Login(request);
        return Ok(token);
    }

    [HttpPost("logoutAll")]
    [Authorize]
    public async Task<IActionResult> LogoutAll()
    {
        var userGuid = (Guid)HttpContext.Items["UserId"]!;

        await userService.InvalidateUserTokens(userGuid);
        return Ok();
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetUserProfile()
    {
        var userGuid = (Guid)HttpContext.Items["UserId"]!;

        var result = await userService.GetUserProfile(userGuid);
        return Ok(result);
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> EditUserProfile(UserEditDto userEditDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.ValidationState);

        var userGuid = (Guid)HttpContext.Items["UserId"]!;

        await userService.EditUserProfile(userGuid, userEditDto);
        return Ok();
    }
}