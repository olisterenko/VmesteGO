using AutoMapper;
using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Responses;
using VmesteGO.Extensions;
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
            $"Вы получили запрос в друзья от {sender.Username}."
        );
    }

    public async Task AcceptFriendRequestAsync(int receiverId, int requestId)
    {
        var spec = new FriendRequestWithUsersSpec(requestId);
        var friendRequest = await _friendRequestRepository.FirstAsync(spec);

        if (friendRequest.ReceiverId != receiverId)
            throw new UnauthorizedAccessException("You are not authorized to accept this friend request.");

        if (friendRequest.Status != FriendRequestStatus.Pending && friendRequest.Status != FriendRequestStatus.Rejected)
            throw new InvalidOperationException("Friend request is not pending.");

        friendRequest.Status = FriendRequestStatus.Accepted;
        await _friendRequestRepository.SaveChangesAsync();

        await _notificationService.AddNotificationAsync(
            friendRequest.SenderId,
            $"Ваш запрос в друзья {friendRequest.Receiver.Username} был принят."
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
            $"Ваш запрос в друзья {friendRequest.Receiver.Username} был отклонен."
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
            fr.Sender.ToUserResponse(_s3Service.GetImageUrl),
            fr.Receiver.ToUserResponse(_s3Service.GetImageUrl),
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
            fr.Sender.ToUserResponse(_s3Service.GetImageUrl),
            fr.Receiver.ToUserResponse(_s3Service.GetImageUrl),
            fr.CreatedAt,
            fr.Status
        ));
    }

    public async Task<FriendRequestResponse?> GetFriendRequest(int fromId, int toId)
    {
        var specification = new GetFriendRequestForUsersSpec(fromId, toId);
        var request = await _friendRequestRepository.SingleOrDefaultAsync(specification);

        return request is null
            ? null
            : new FriendRequestResponse(
                request.Id,
                request.Sender.ToUserResponse(_s3Service.GetImageUrl),
                request.Receiver.ToUserResponse(_s3Service.GetImageUrl),
                request.CreatedAt,
                request.Status
            );
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
            $"Вы были удалены из друзей {user.Username}."
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
            $"Запрос в друзья от {friendRequest.Sender.Username} был отозван.",
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
                EventResponse = g.Key.ToEventResponse(_s3Service.GetImageUrl),
                Friends = g.Select(ea => ea.User.ToUserResponse(_s3Service.GetImageUrl)).Distinct()
            })
            .ToList();

        return groupedEvents;
    }

    public async Task<List<FriendEventStatusResponse>> GetFriendEventByIdAsync(int userId, int eventId)
    {
        var specification = new FriendsOfUserSpec(userId);
        var friends = await _friendRequestRepository.ListAsync(specification);

        if (friends.Count == 0) return [];
        
        var friendIds = friends.Select(f => f.ReceiverId == userId ? f.SenderId : f.ReceiverId).Distinct().ToList();
        var userEvents = await _userEventRepository.ListAsync(new FriendsSpecificEventSpecification(eventId, friendIds));

        var usersWithStatuses = userEvents.Select(
            x => new FriendEventStatusResponse
            {
                Friend = x.User.ToUserResponse(_s3Service.GetImageUrl),
                EventStatus = x.EventStatus
            }).ToList();
        var usersWithoutStatuses = friends
            .Where(f => userEvents.All(ue => ue.UserId != (f.ReceiverId == userId ? f.SenderId : f.ReceiverId)))
            .Select(
            f => new FriendEventStatusResponse
            {
                Friend = (f.ReceiverId == userId ? f.Sender : f.Receiver).ToUserResponse(_s3Service.GetImageUrl),
                EventStatus = null
            }).ToList();

        return usersWithStatuses.Union(usersWithoutStatuses).ToList();
    }
}