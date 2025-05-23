using VmesteGO.Dto.Responses;

namespace VmesteGO.Services.Interfaces;

public interface IFriendService
{
    Task SendFriendRequestAsync(int senderId, int receiverId);
    Task AcceptFriendRequestAsync(int receiverId, int requestId);
    Task RejectFriendRequestAsync(int receiverId, int requestId);
    Task<IEnumerable<FriendResponse>> GetFriendsAsync(int userId);
    Task<IEnumerable<FriendRequestResponse>> GetPendingFriendRequestsAsync(int userId);
    Task<IEnumerable<FriendRequestResponse>> GetSentFriendRequestsAsync(int userId);
    Task<FriendRequestResponse?> GetFriendRequest(int fromId, int toId);
    Task RemoveFriendAsync(int userId, int friendId);
    Task RevokeFriendRequestAsync(int requestId, int userId, CancellationToken cancellationToken);
    Task<IEnumerable<FriendEventResponse>> GetFriendsEventsAsync(int userId);
    Task<List<FriendEventStatusResponse>> GetFriendEventByIdAsync(int userId, int eventId);
}