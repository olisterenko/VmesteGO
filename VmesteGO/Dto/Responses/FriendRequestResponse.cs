using VmesteGO.Domain.Enums;

namespace VmesteGO.Dto.Responses;

public record FriendRequestResponse(
    int Id,
    UserResponse Sender,
    UserResponse Receiver,
    DateTime CreatedAt,
    FriendRequestStatus Status);