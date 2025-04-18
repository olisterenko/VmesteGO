using AutoMapper;
using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Responses;
using VmesteGO.Services.Interfaces;
using VmesteGO.Specifications.FriendRequestSpecs;
using VmesteGO.Specifications.UserEventSpecs;

namespace VmesteGO.Services;

public class FriendService : IFriendService
{
    private readonly IRepository<FriendRequest> _friendRequestRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<UserEvent> _userEventRepository;
    private readonly IMapper _mapper;
    private readonly IS3StorageService _s3Service;
    private readonly INotificationService _notificationService;

    public FriendService(
        IRepository<FriendRequest> friendRequestRepository,
        IRepository<User> userRepository,
        IRepository<UserEvent> userEventRepository,
        IMapper mapper,
        IS3StorageService s3Service, 
        INotificationService notificationService)
    {
        _friendRequestRepository = friendRequestRepository;
        _userRepository = userRepository;
        _userEventRepository = userEventRepository;
        _mapper = mapper;
        _s3Service = s3Service;
        _notificationService = notificationService;
    }

    public async Task SendFriendRequestAsync(int senderId, int receiverId)
    {
        if (senderId == receiverId)
            throw new InvalidOperationException("Cannot send friend request to yourself.");

        var sender = await _userRepository.GetByIdAsync(senderId);
        var receiver = await _userRepository.GetByIdAsync(receiverId);

        if (sender == null || receiver == null)
            throw new KeyNotFoundException("Sender or receiver not found.");

        var existingFriendship = await _friendRequestRepository.FirstOrDefaultAsync(
            new CheckExistingFriendRequestSpec(senderId, receiverId));

        if (existingFriendship != null)
            throw new InvalidOperationException("You are already friends or a request is pending.");

        var friendRequest = new FriendRequest
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Status = FriendRequestStatus.Pending
        };

        await _friendRequestRepository.AddAsync(friendRequest);
        await _friendRequestRepository.SaveChangesAsync();
        
        await _notificationService.AddNotificationAsync(
            receiver.Id,
            $"You received a friend request from {sender.Username}."
        );
    }

    public async Task AcceptFriendRequestAsync(int receiverId, int requestId)
    {
        var spec = new FriendRequestWithUsersSpec(requestId);
        var friendRequest = await _friendRequestRepository.FirstAsync(spec);

        if (friendRequest.ReceiverId != receiverId)
            throw new UnauthorizedAccessException("You are not authorized to accept this friend request.");

        if (friendRequest.Status != FriendRequestStatus.Pending)
            throw new InvalidOperationException("Friend request is not pending.");

        friendRequest.Status = FriendRequestStatus.Accepted;
        await _friendRequestRepository.SaveChangesAsync();
        
        await _notificationService.AddNotificationAsync(
            friendRequest.SenderId,
            $"Your friend request for {friendRequest.Receiver.Username} was accepted."
        );
    }

    public async Task RejectFriendRequestAsync(int receiverId, int requestId)
    {
        var spec = new FriendRequestWithUsersSpec(requestId);
        var friendRequest = await _friendRequestRepository.FirstAsync(spec);

        if (friendRequest.ReceiverId != receiverId)
            throw new UnauthorizedAccessException("You are not authorized to reject this friend request.");

        if (friendRequest.Status != FriendRequestStatus.Pending)
            throw new InvalidOperationException("Friend request is not pending.");

        friendRequest.Status = FriendRequestStatus.Rejected;
        await _friendRequestRepository.SaveChangesAsync();
        
        await _notificationService.AddNotificationAsync(
            friendRequest.SenderId,
            $"Your friend request for {friendRequest.Receiver.Username} was rejected."
        );
    }

    public async Task<IEnumerable<FriendResponse>> GetFriendsAsync(int userId)
    {
        var specification = new FriendsOfUserSpec(userId);
        var friends = await _friendRequestRepository.ListAsync(specification);

        var friendResponses = friends.Select(f => new FriendResponse(
            f.Id,
            f.ReceiverId == userId ? f.SenderId : f.ReceiverId,
            f.ReceiverId == userId ? f.Sender.Username : f.Receiver.Username,
            f.ReceiverId == userId
                ? _s3Service.GetImageUrl(f.Sender.ImageKey)
                : _s3Service.GetImageUrl(f.Receiver.ImageKey)
        ));

        return friendResponses;
    }

    public async Task<IEnumerable<FriendRequestResponse>> GetPendingFriendRequestsAsync(int userId)
    {
        var specification = new PendingFriendRequestsSpec(userId);
        var requests = await _friendRequestRepository.ListAsync(specification);

        return requests.Select(fr => new FriendRequestResponse(
            fr.Id,
            fr.SenderId,
            fr.Sender.Username,
            fr.ReceiverId,
            fr.Receiver.Username,
            fr.CreatedAt,
            fr.Status
        ));
    }

    public async Task<IEnumerable<FriendRequestResponse>> GetSentFriendRequestsAsync(int userId)
    {
        var specification = new SentFriendRequestsSpec(userId);
        var requests = await _friendRequestRepository.ListAsync(specification);

        return requests.Select(fr => new FriendRequestResponse(
            fr.Id,
            fr.SenderId,
            fr.Sender.Username,
            fr.ReceiverId,
            fr.Receiver.Username,
            fr.CreatedAt,
            fr.Status
        ));
    }

    public async Task RemoveFriendAsync(int userId, int friendId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        var specification = new CheckExistingFriendshipSpec(userId, friendId);
        var friendship = await _friendRequestRepository.FirstAsync(specification);

        if (userId == friendship.SenderId)
        {
            friendship.Status = FriendRequestStatus.Pending;
        }

        if (userId == friendship.ReceiverId)
        {
            friendship.Status = FriendRequestStatus.Rejected;
        }

        await _friendRequestRepository.SaveChangesAsync();
        
        await _notificationService.AddNotificationAsync(
            friendId,
            $"You were deleted from friends by {user.Username}."
        );
    }

    public async Task RevokeFriendRequestAsync(int requestId, int userId, CancellationToken cancellationToken)
    {
        var spec = new FriendRequestWithUsersSpec(requestId);
        var friendRequest = await _friendRequestRepository.FirstAsync(spec, cancellationToken);

        if (friendRequest.SenderId != userId)
            throw new UnauthorizedAccessException("You are not authorized to revoke to this invitation");

        await _friendRequestRepository.DeleteAsync(friendRequest, cancellationToken);
        
        await _notificationService.AddNotificationAsync(
            friendRequest.ReceiverId,
            $"Friend request by {friendRequest.Sender.Username} was revoked.",
            cancellationToken
        );
    }

    public async Task<IEnumerable<FriendEventResponse>> GetFriendsEventsAsync(int userId)
    {
        var specification = new FriendsOfUserSpec(userId);
        var friends = await _friendRequestRepository.ListAsync(specification);

        if (friends.Count == 0) return [];

        var friendIds = friends.Select(f => f.ReceiverId == userId ? f.SenderId : f.ReceiverId).Distinct();
        var userEvents = await _userEventRepository.ListAsync(new FriendsEventsSpecification(friendIds));

        var groupedEvents = userEvents
            .GroupBy(ea => ea.Event)
            .Select(g => new FriendEventResponse
            {
                EventResponse = _mapper.Map<EventResponse>(g.Key),
                Friends = g.Select(ea => _mapper.Map<UserResponse>(ea.User)).Distinct()
            })
            .ToList();

        return groupedEvents;
    }
}