namespace VmesteGO.Dto.Responses;

public class FriendEventResponse
{
    public EventResponse EventResponse { get; set; } = null!;
    public IEnumerable<UserResponse> Friends { get; set; } = null!;
}