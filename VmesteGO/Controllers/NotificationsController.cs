using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VmesteGO.Services.Interfaces;

namespace VmesteGO.Controllers;

[Authorize]
[ApiController]
[Route("notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IUserContext _userContext;

    public NotificationsController(INotificationService notificationService, IUserContext userContext)
    {
        _notificationService = notificationService;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotificationsForUser([FromQuery] bool? isRead, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.UserId;
        var notifications = await _notificationService.GetNotificationsForUserAsync(userId, isRead, cancellationToken);
        return Ok(notifications);
    }

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.UserId;
        await _notificationService.MarkAsReadAsync(id, userId, cancellationToken);
        return Ok();
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken = default)
    {
        var userId = _userContext.UserId;
        await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
        return Ok();
    }
}