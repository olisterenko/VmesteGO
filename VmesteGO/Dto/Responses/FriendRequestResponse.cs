using VmesteGO.Domain.Enums;

namespace VmesteGO.Dto.Responses;

public record FriendRequestResponse(
    int Id,
    int SenderId,
    string SenderUsername,
    int ReceiverId,
    string ReceiverUsername,
    DateTime CreatedAt,
    FriendRequestStatus Status);