using System.Security.Claims;
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
    private readonly IUserContext _userContext;

    public UsersController(IUserService userService, IUserContext userContext)
    {
        _userService = userService;
        _userContext = userContext;
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
        var currentUserId =
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"); // TODO: заменить на UserContext
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (currentUserId != id && currentUserRole != Role.Admin.ToString())
            return Forbid();

        var updatedUser = await _userService.UpdateUserAsync(id, request);
        return Ok(updatedUser);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (currentUserId != id && currentUserRole != Role.Admin.ToString())
            return Forbid();

        var deleted = await _userService.DeleteUserAsync(id);
        if (!deleted)
            return BadRequest();

        return NoContent();
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] UserSearchRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        request.CurrentUserId = currentUserId;
        var users = await _userService.SearchUsersAsync(request, cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:int}/images-upload")]
    public async Task<IActionResult> GetUserUploadUrl(int id)
    {
        var currentUserId = _userContext.UserId;

        if (currentUserId != id)
            return Forbid();
        
        var uploadInfo =  await _userService.GetUserUploadUrl(id);
        return Ok(uploadInfo);
    }
    
    [HttpPost("{id:int}/confirm-image-upload")]
    public async Task<IActionResult> GetUserUploadUrl(int id, [FromBody] UserConfirmImageUploadRequest request)
    {
        var currentUserId = _userContext.UserId;

        if (currentUserId != id)
            return Forbid();
        
        var userInfo =  await _userService.UpdateUserImageKey(id, request.Key);
        return Ok(userInfo);
    }
}