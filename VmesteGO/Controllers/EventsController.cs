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

    public EventsController(IEventService eventService, ILogger<EventsController> logger)
    {
        _eventService = eventService;
        _logger = logger;
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
        if (includePrivate)
        {
            // Ensure the requesting user is admin
            if (User.Identity?.IsAuthenticated != true || !User.IsInRole("admin"))
            {
                return Forbid();
            }
        }

        var events = await _eventService.GetAllEventsAsync(includePrivate);
        return Ok(events);
    }
    
    // TODO: список мероприятий друзей
    
    // TODO: поиск мероприятия
    
    // TODO: смена пользовательского статуса мероприятия

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
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var roleString = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(roleString))
        {
            return Unauthorized();
        }

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }
        
        var role = Enum.Parse<Role>(roleString!);

        try
        {
            var evt = await _eventService.CreateEventAsync(createDto, userId, role);
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
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var roleString = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(roleString))
        {
            return Unauthorized();
        }

        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }
        
        var role = Enum.Parse<Role>(roleString!);

        try
        {
            var evt = await _eventService.UpdateEventAsync(id, updateDto, userId, role);
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
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var roleString = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(roleString))
        {
            return Unauthorized();
        }

        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }
        
        var role = Enum.Parse<Role>(roleString!);

        try
        {
            await _eventService.DeleteEventAsync(id, userId, role);
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
}