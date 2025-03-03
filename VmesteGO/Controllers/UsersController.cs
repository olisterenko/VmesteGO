using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Requests;
using VmesteGO.Dto.Responses;
using VmesteGO.Services.Interfaces;

namespace VmesteGO.Controllers;

[Route("users")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsersController(IUserService userService, IHttpContextAccessor httpContextAccessor)
    {
        _userService = userService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponse>> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return Ok(user);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(int id, UserUpdateRequest request)
    {
        var currentUserId = int.Parse(User.FindFirst("id")?.Value ?? "0");
        var currentUserRole = User.FindFirst("role")?.Value;

        if (currentUserId != id && currentUserRole != Role.Admin.ToString())
            return Forbid();

        var updatedUser = await _userService.UpdateUserAsync(id, request);
        return Ok(updatedUser);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var currentUserId = int.Parse(User.FindFirst("id")?.Value ?? "0");
        var currentUserRole = User.FindFirst("role")?.Value;

        if (currentUserId != id && currentUserRole != Role.Admin.ToString())
            return Forbid();

        var deleted = await _userService.DeleteUserAsync(id);
        if (!deleted)
            return BadRequest();

        return NoContent();
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] UserSearchRequest request, CancellationToken cancellationToken)
    {
        var users = await _userService.SearchUsersAsync(request, cancellationToken);
        return Ok(users);
    }
}