using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Requests;
using VmesteGO.Dto.Responses;
using VmesteGO.Services.Interfaces;

namespace VmesteGO.Controllers;

[Route("events-invitations")]
[ApiController]
[Authorize]
public class EventInvitationsController : ControllerBase
{
    private readonly IEventInvitationService _invitationService;
    private readonly IUserContext _userContext;

    public EventInvitationsController(IEventInvitationService invitationService, IUserContext userContext)
    {
        _invitationService = invitationService;
        _userContext = userContext;
    }

    [HttpPost("invite")]
    public async Task<IActionResult> InviteUser([FromBody] CreateInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var senderId = _userContext.UserId;
        await _invitationService.InviteUserAsync(request.EventId, request.ReceiverId, senderId, cancellationToken);
        return Ok(new { message = "Invitation sent successfully" });
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<InvitationResponse>>> GetPendingEventInvitations(CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;
        var invitations = await _invitationService.GetPendingEventInvitationsAsync(userId, cancellationToken);
        return Ok(invitations);
    }

    [HttpGet("sent")]
    public async Task<ActionResult<IEnumerable<InvitationResponse>>> GetSentFriendRequests(CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;
        var requests = await _invitationService.GetSentEventInvitationsAsync(userId, cancellationToken);
        return Ok(requests);
    }

    [HttpPost("{id:int}/accept")]
    public async Task<IActionResult> AcceptInvitation(int id, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;
        await _invitationService.RespondToInvitationAsync(id, EventInvitationStatus.Accepted, userId,
            cancellationToken);
        return Ok(new { message = "Invitation accepted" });
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> RejectInvitation(int id, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;
        await _invitationService.RespondToInvitationAsync(id, EventInvitationStatus.Rejected, userId,
            cancellationToken);
        return Ok(new { message = "Invitation rejected" });
    }

    [HttpDelete("{invitationId:int}")]
    public async Task<IActionResult> RevokeInvitation(int invitationId, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;
        await _invitationService.RevokeInvitationAsync(invitationId, userId, cancellationToken);
        return Ok(new { message = "Invitation revoked" });
    }
}