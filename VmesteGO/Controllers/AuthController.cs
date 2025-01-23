using Microsoft.AspNetCore.Mvc;
using VmesteGO.Dto.Requests;
using VmesteGO.Services.Interfaces;

namespace VmesteGO.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterRequest userRegisterRequest)
    {
        try
        {
            var token = await _userService.RegisterUser(userRegisterRequest);
            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginRequest userLoginRequest)
    {
        try
        {
            var token = await _userService.LoginUser(userLoginRequest);
            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}