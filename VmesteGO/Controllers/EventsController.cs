using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Requests;
using VmesteGO.Dto.Responses;
using VmesteGO.Services.Interfaces;

namespace VmesteGO.Controllers;

[ApiController]
[Route("events")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly ILogger<EventsController> _logger;
    private readonly IUserContext _userContext;

    public EventsController(IEventService eventService, ILogger<EventsController> logger, IUserContext userContext)
    {
        _eventService = eventService;
        _logger = logger;
        _userContext = userContext;
    }

    // TODO: как-то тут переделать, чтобы в параметры эвентСтатус и айдишник пользователя подпихивать. Простой список в книжках подглядеть
    /// <summary>
    /// Retrieves all events. Optionally includes private events if requested by an admin.
    /// </summary>
    /// <param name="includePrivate">Set to true to include private events (admins only).</param>
    /// <returns>List of events.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<EventResponse>>> GetEvents([FromQuery] bool includePrivate = false)
    {
        // TODO: переделать
        /*if (includePrivate)
        {
            // Ensure the requesting user is admin
            if (User.Identity?.IsAuthenticated != true || !User.IsInRole("admin"))
            {
                return Forbid();
            }
        }*/

        var events = await _eventService.GetAllEventsAsync(includePrivate);
        return Ok(events);
    }

    [HttpPost("{eventId:int}/status")]
    public async Task<ActionResult> ChangeEventStatus(int eventId, [FromBody] EventStatus eventStatus)
    {
        var userId = _userContext.UserId;

        await _eventService.ChangeEventStatus(
            new ChangeEventStatusRequest
            {
                UserId = userId,
                EventId = eventId,
                NewEventStatus = eventStatus
            });

        return Ok();
    }

    /// <summary>
    /// Retrieves a specific event by its ID.
    /// </summary>
    /// <param name="id">Event ID.</param>
    /// <returns>Event details.</returns>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<EventResponse>> GetEvent(int id)
    {
        var evt = await _eventService.GetEventByIdAsync(id);

        return Ok(evt);
    }

    /// <summary>
    /// Creates a new event. Admins can create public or private events; users can only create private events.
    /// </summary>
    /// <param name="createDto">Event creation details.</param>
    /// <returns>Created event.</returns>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<EventResponse>> CreateEvent([FromBody] CreateEventRequest createDto)
    {
        var userId = _userContext.UserId;
        var role = _userContext.Role;

        if (!Enum.TryParse(role, out Role userRole))
        {
            return Unauthorized();
        }

        try
        {
            var evt = await _eventService.CreateEventAsync(createDto, userId, userRole);
            return CreatedAtAction(nameof(GetEvent), new { id = evt.Id }, evt);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to create event.");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event.");
            return StatusCode(500, "Internal server error.");
        }
    }

    /// <summary>
    /// Updates an existing event. Admins can update any event; users can only update their own events.
    /// </summary>
    /// <param name="id">Event ID.</param>
    /// <param name="updateDto">Event update details.</param>
    /// <returns>Updated event.</returns>
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<EventResponse>> UpdateEvent(int id, [FromBody] UpdateEventRequest updateDto)
    {
        var userId = _userContext.UserId;
        var role = _userContext.Role;

        if (!Enum.TryParse(role, out Role userRole))
        {
            return Unauthorized();
        }

        try
        {
            var evt = await _eventService.UpdateEventAsync(id, updateDto, userId, userRole);
            return Ok(evt);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Event not found.");
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to update event.");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event.");
            return StatusCode(500, "Internal server error.");
        }
    }

    /// <summary>
    /// Deletes an event. Admins can delete any event; users can only delete their own events.
    /// </summary>
    /// <param name="id">Event ID.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var userId = _userContext.UserId;
        var role = _userContext.Role;

        if (!Enum.TryParse(role, out Role userRole))
        {
            return Unauthorized();
        }

        try
        {
            await _eventService.DeleteEventAsync(id, userId, userRole);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Event not found.");
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to delete event.");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event.");
            return StatusCode(500, "Internal server error.");
        }
    }

    // TODO: проверить поиски
    [HttpGet("created-private")]
    [Authorize]
    public async Task<IActionResult> GetCreatedPrivateEvents(
        [FromQuery] string q,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 10)
    {
        var userId = _userContext.UserId;
        var events = await _eventService.GetCreatedPrivateEventsAsync(userId, q, offset, limit);
        return Ok(events);
    }

    [HttpGet("joined-private")]
    [Authorize]
    public async Task<IActionResult> GetJoinedPrivateEvents(
        [FromQuery] string q,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 10)
    {
        var userId = _userContext.UserId;
        var events = await _eventService.GetJoinedPrivateEventsAsync(userId, q, offset, limit);
        return Ok(events);
    }

    [HttpGet("created-public")]
    public async Task<IActionResult> GetCreatedPublicEvents(
        [FromQuery] string q,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 10)
    {
        var role = _userContext.Role;

        if (!Enum.TryParse(role, out Role userRole))
        {
            return Unauthorized();
        }

        if (userRole != Role.Admin)
        {
            return Ok(new List<EventResponse>());
        }

        var userId = _userContext.UserId;
        var events = await _eventService.GetCreatedPublicEventsAsync(userId, q, offset, limit);
        return Ok(events);
    }

    [HttpGet("other-admins-public")]
    public async Task<IActionResult> GetOtherAdminsPublicEvents(
        [FromQuery] string q,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 10)
    {
        var userId = _userContext.UserId;
        var events = await _eventService.GetOtherAdminsPublicEventsAsync(userId, q, offset, limit);
        return Ok(events);
    }
    
    [HttpGet("{eventId:int}/images-upload/url")]
    public async Task<IActionResult> GetEventUploadUrl(int eventId)
    {
        var userId = _userContext.UserId;
        var role = _userContext.Role;

        if (!Enum.TryParse(role, out Role userRole))
        {
            return Unauthorized();
        }
        
        var uploadInfo = await _eventService.GetEventUploadUrl(eventId, userId, userRole);

        return Ok(uploadInfo);
    }
    
    [HttpPost("{eventId:int}/images-upload/confirm")]
    public async Task<IActionResult> ConfirmEventImageUpload(int eventId, [FromBody] ImageUploadConfirmationRequest dto)
    {
        var userId = _userContext.UserId;
        var role = _userContext.Role;

        if (!Enum.TryParse(role, out Role userRole))
        {
            return Unauthorized();
        }
        
        await _eventService.SaveImageMetadataAsync(eventId, dto.ImageKey, dto.OrderIndex, userId, userRole);
        return Ok(new { message = "Image saved successfully" });
    }
    
    [HttpDelete("/images/{imageId:int}")]
    public async Task<IActionResult> DeleteEventImage(int imageId)
    {
        await _eventService.DeleteImageAsync(imageId);
        return Ok(new { message = "Image deleted successfully" });
    }
}