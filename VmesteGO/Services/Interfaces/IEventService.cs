using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Requests;
using VmesteGO.Dto.Responses;

namespace VmesteGO.Services.Interfaces;

public interface IEventService
{
    Task<EventResponse> GetEventByIdAsync(int id);
    Task<IEnumerable<EventResponse>> GetAllEventsAsync(bool includePrivate = false);
    Task<EventResponse> CreateEventAsync(CreateEventRequest createDto, int creatorId, Role role);
    Task<EventResponse> UpdateEventAsync(int id, UpdateEventRequest updateDto, int userId, Role role);
    Task DeleteEventAsync(int id, int userId, Role role);
}