using VmesteGO.Domain.Enums;

namespace VmesteGO.Dto.Responses;

public class FriendEventStatusResponse
{
    public EventStatus? EventStatus { get; set; }
    public required UserResponse Friend { get; set; }
}