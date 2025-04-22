using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using VmesteGO.Dto.Requests;
using VmesteGO.Filters;
using VmesteGO.Services.Interfaces;

namespace VmesteGO.Controllers;

[ApiController]
[Route("auth")]
[ValidationExceptionFilter]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<UserRegisterRequest> _registerValidator;
    private readonly IValidator<UserLoginRequest> _loginValidator;

    public AuthController(
        IUserService userService,
        IValidator<UserRegisterRequest> registerValidator,
        IValidator<UserLoginRequest> loginValidator)
    {
        _userService = userService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterRequest userRegisterRequest)
    {
        try
        {
            await _registerValidator.ValidateAndThrowAsync(userRegisterRequest);

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
            await _loginValidator.ValidateAndThrowAsync(userLoginRequest);

            var token = await _userService.LoginUser(userLoginRequest);
            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}