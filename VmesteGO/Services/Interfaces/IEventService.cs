using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Requests;
using VmesteGO.Dto.Responses;

namespace VmesteGO.Services.Interfaces;

public interface IEventService
{
    Task<EventResponse> GetEventByIdAsync(int id);
    Task<EventResponse> GetEventByIdForUserAsync(int userId, int eventId);

    Task<IEnumerable<EventResponse>> GetEventsAsync(GetEventsRequest getEventsRequest);
    Task<EventResponse> CreateEventAsync(CreateEventRequest createDto, int creatorId, Role role);
    Task<EventResponse> UpdateEventAsync(int id, UpdateEventRequest updateDto, int userId, Role role);
    Task DeleteEventAsync(int id, int userId, Role role);
    Task ChangeEventStatus(ChangeEventStatusRequest changeEventStatusRequest);

    Task<IEnumerable<EventResponse>> GetCreatedPrivateEventsAsync(
        int userId,
        string? q,
        List<int>? categoryIds,
        int offset,
        int limit);

    Task<IEnumerable<EventResponse>> GetJoinedPrivateEventsAsync(
        int userId,
        string? q,
        List<int>? categoryIds,
        int offset,
        int limit);

    Task<IEnumerable<EventResponse>> GetCreatedPublicEventsAsync(
        int userId,
        string? q,
        List<int>? categoryIds,
        int offset,
        int limit);

    Task<IEnumerable<EventResponse>> GetOtherAdminsPublicEventsAsync(
        int userId,
        string? q,
        List<int>? categoryIds,
        int offset,
        int limit);

    Task<UploadEventImageUrlResponse> GetEventUploadUrl(int id, int userId, Role role);
    Task SaveImageMetadataAsync(int eventId, string imageKey, int orderIndex, int userId, Role role);
    Task DeleteImageAsync(int imageId);

    Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync();
}