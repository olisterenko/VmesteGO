using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Requests;
using VmesteGO.Dto.Responses;
using VmesteGO.Services.Interfaces;

namespace VmesteGO.Controllers;

[ApiController]
[Route("events")]
[Authorize]
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventResponse>>> GetEvents(
        [FromQuery] int? userId = null,
        [FromQuery] EventStatus? eventStatus = null)
    {
        var includePrivate = userId == null && _userContext.UserIdUnsafe != null;
        var userIdUnsafe = userId ?? _userContext.UserIdUnsafe;
        var events = await _eventService.GetEventsAsync(
            new GetEventsRequest
            {
                UserId = userIdUnsafe,
                EventStatus = eventStatus,
                IncludePrivate = includePrivate
            }
        );

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

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventResponse>> GetEvent(int id)
    {
        var userId = _userContext.UserId;
        var evt = await _eventService.GetEventByIdForUserAsync(userId, id);

        return Ok(evt);
    }

    [HttpPost]
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

    [HttpPut("{id:int}")]
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

    [HttpDelete("{id:int}")]
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

    [HttpGet("created-private")]
    public async Task<IActionResult> GetCreatedPrivateEvents(
        [FromQuery] string? q,
        [FromQuery] List<int>? categoryIds,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 10)
    {
        var userId = _userContext.UserId;
        var events = await _eventService.GetCreatedPrivateEventsAsync(userId, q, categoryIds, offset, limit);
        return Ok(events);
    }

    [HttpGet("joined-private")]
    public async Task<IActionResult> GetJoinedPrivateEvents(
        [FromQuery] string? q,
        [FromQuery] List<int>? categoryIds,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 10)
    {
        var userId = _userContext.UserId;
        var events = await _eventService.GetJoinedPrivateEventsAsync(userId, q, categoryIds, offset, limit);
        return Ok(events);
    }

    [HttpGet("created-public")]
    public async Task<IActionResult> GetCreatedPublicEvents(
        [FromQuery] string? q,
        [FromQuery] List<int>? categoryIds,
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
        var events = await _eventService.GetCreatedPublicEventsAsync(userId, q, categoryIds, offset, limit);
        return Ok(events);
    }

    [HttpGet("other-admins-public")]
    public async Task<IActionResult> GetOtherAdminsPublicEvents(
        [FromQuery] string? q,
        [FromQuery] List<int>? categoryIds,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 10)
    {
        var userId = _userContext.UserId;
        var events = await _eventService.GetOtherAdminsPublicEventsAsync(userId, q, categoryIds, offset, limit);
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
    
    [HttpGet("categories")]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _eventService.GetAllCategoriesAsync();
        return Ok(categories);
    }
}