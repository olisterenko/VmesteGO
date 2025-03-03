using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VmesteGO.Dto.Requests;
using VmesteGO.Services.Interfaces;

namespace VmesteGO.Controllers;

[ApiController]
[Route("friends")]
[Authorize]
public class FriendsController : ControllerBase
{
    private readonly IFriendService _friendService;
    private readonly IUserContext _userContext;

    public FriendsController(IFriendService friendService, IUserContext userContext)
    {
        _friendService = friendService;
        _userContext = userContext;
    }

    [HttpPost("requests")]
    public async Task<IActionResult> SendFriendRequest([FromBody] SendFriendRequestRequest dto)
    {
        var userId = _userContext.UserId;
        await _friendService.SendFriendRequestAsync(userId, dto.ReceiverId);
        return Ok(new { message = "Friend request sent." });
    }

    [HttpPost("requests/{requestId:int}/accept")]
    public async Task<IActionResult> AcceptFriendRequest(int requestId)
    {
        var userId = _userContext.UserId;
        await _friendService.AcceptFriendRequestAsync(userId, requestId);
        return Ok(new { message = "Friend request accepted." });
    }

    [HttpPost("requests/{requestId:int}/reject")]
    public async Task<IActionResult> RejectFriendRequest(int requestId)
    {
        var userId = _userContext.UserId;
        await _friendService.RejectFriendRequestAsync(userId, requestId);
        return Ok(new { message = "Friend request rejected." });
    }

    [HttpDelete("requests/{requestId:int}")]
    public async Task<IActionResult> RevokeFriendRequest(int requestId, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;
        await _friendService.RevokeFriendRequestAsync(requestId, userId, cancellationToken);
        return Ok(new { message = "Friend request revoked" });
    }

    [HttpGet("friends")]
    public async Task<IActionResult> GetFriends()
    {
        var userId = _userContext.UserId;
        var friends = await _friendService.GetFriendsAsync(userId);
        return Ok(friends);
    }

    [HttpGet("requests/pending")]
    public async Task<IActionResult> GetPendingFriendRequests()
    {
        var userId = _userContext.UserId;
        var requests = await _friendService.GetPendingFriendRequestsAsync(userId);
        return Ok(requests);
    }

    [HttpGet("requests/sent")]
    public async Task<IActionResult> GetSentFriendRequests()
    {
        var userId = _userContext.UserId;
        var requests = await _friendService.GetSentFriendRequestsAsync(userId);
        return Ok(requests);
    }

    [HttpDelete("friends/{friendId:int}")]
    public async Task<IActionResult> RemoveFriend(int friendId)
    {
        var userId = _userContext.UserId;
        await _friendService.RemoveFriendAsync(userId, friendId);
        return Ok(new { message = "Friend removed." });
    }
}