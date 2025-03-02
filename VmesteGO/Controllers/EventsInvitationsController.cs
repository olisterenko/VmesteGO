using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Requests;
using VmesteGO.Services.Interfaces;

namespace VmesteGO.Controllers;

[Route("events-invitations")]
[ApiController]
[Authorize]
public class EventInvitationsController : ControllerBase
{
    private readonly IEventInvitationService _invitationService;

    public EventInvitationsController(IEventInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    /// <summary>
    /// Invite a user to an event.
    /// </summary>
    /// <param name="request">Invitation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Status message.</returns>
    [HttpPost("invite")]
    public async Task<IActionResult> InviteUser([FromBody] CreateInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var senderId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _invitationService.InviteUserAsync(request.EventId, request.ReceiverId, senderId, cancellationToken);
        return Ok(new { message = "Invitation sent successfully" });
    }

    /// <summary>
    /// Get invitations for the authenticated user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of invitations.</returns>
    [HttpGet("sent")]
    public async Task<IActionResult> GetMyInvitations(CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var invitations = await _invitationService.GetInvitationsForUserAsync(userId, cancellationToken);
        return Ok(invitations);
    }
    
    // TODO: список исходящих заявок

    /// <summary>
    /// Accept an invitation.
    /// </summary>
    /// <param name="id">Invitation ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Status message.</returns>
    [HttpPost("{id:int}/accept")]
    public async Task<IActionResult> AcceptInvitation(int id, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _invitationService.RespondToInvitationAsync(id, EventInvitationStatus.Accepted, userId,
            cancellationToken);
        return Ok(new { message = "Invitation accepted" });
    }

    /// <summary>
    /// Reject an invitation.
    /// </summary>
    /// <param name="id">Invitation ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Status message.</returns>
    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> RejectInvitation(int id, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _invitationService.RespondToInvitationAsync(id, EventInvitationStatus.Rejected, userId,
            cancellationToken);
        return Ok(new { message = "Invitation rejected" });
    }
    
    // TODO: отменить приглашение
}