using AutoMapper;
using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Responses;
using VmesteGO.Services.Interfaces;
using VmesteGO.Specifications.FriendRequestSpecs;
using VmesteGO.Specifications.FriendSpecs;

namespace VmesteGO.Services;

public class FriendService : IFriendService
{
    private readonly IRepository<FriendRequest> _friendRequestRepository;
    private readonly IRepository<Friend> _friendRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IMapper _mapper;

    public FriendService(
        IRepository<FriendRequest> friendRequestRepository,
        IRepository<Friend> friendRepository,
        IRepository<User> userRepository,
        IMapper mapper)
    {
        _friendRequestRepository = friendRequestRepository;
        _friendRepository = friendRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task SendFriendRequestAsync(int senderId, int receiverId)
    {
        if (senderId == receiverId)
            throw new InvalidOperationException("Cannot send friend request to yourself.");

        var sender = await _userRepository.GetByIdAsync(senderId);
        var receiver = await _userRepository.GetByIdAsync(receiverId);

        if (sender == null || receiver == null)
            throw new KeyNotFoundException("Sender or receiver not found.");

        var existingFriendship = await _friendRepository.FirstOrDefaultAsync(
            new CheckExistingFriendshipSpec(senderId, receiverId));

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
    }

    public async Task AcceptFriendRequestAsync(int receiverId, int requestId)
    {
        var friendRequest = await _friendRequestRepository.GetByIdAsync(requestId)
            ?? throw new KeyNotFoundException("Friend request not found.");

        if (friendRequest.ReceiverId != receiverId)
            throw new UnauthorizedAccessException("You are not authorized to accept this friend request.");

        if (friendRequest.Status != FriendRequestStatus.Pending)
            throw new InvalidOperationException("Friend request is not pending.");

        friendRequest.Status = FriendRequestStatus.Accepted;
        await _friendRequestRepository.SaveChangesAsync();

        var friendship = new Friend
        {
            UserId = friendRequest.SenderId,
            FriendUserId = friendRequest.ReceiverId
        };
        await _friendRepository.AddAsync(friendship);

        await _friendRequestRepository.SaveChangesAsync();
        await _friendRepository.SaveChangesAsync();
    }

    public async Task RejectFriendRequestAsync(int receiverId, int requestId)
    {
        var friendRequest = await _friendRequestRepository.GetByIdAsync(requestId)
            ?? throw new KeyNotFoundException("Friend request not found.");

        if (friendRequest.ReceiverId != receiverId)
            throw new UnauthorizedAccessException("You are not authorized to reject this friend request.");

        if (friendRequest.Status != FriendRequestStatus.Pending)
            throw new InvalidOperationException("Friend request is not pending.");

        friendRequest.Status = FriendRequestStatus.Rejected;
        await _friendRequestRepository.SaveChangesAsync();

        await _friendRequestRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<FriendResponse>> GetFriendsAsync(int userId)
    {
        var specification = new FriendsOfUserSpec(userId);
        var friends = await _friendRepository.ListAsync(specification);

        var friendResponses = friends.Select(f => new FriendResponse(
            f.Id,
            f.FriendUserId == userId ? f.UserId : f.FriendUserId,
            f.FriendUserId == userId ? f.User.Username : f.FriendUser.Username,
            f.FriendUserId == userId ? f.User.ImageUrl : f.FriendUser.ImageUrl
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
            fr.CreatedAt,
            fr.Status
        ));
    }

    public async Task RemoveFriendAsync(int userId, int friendId)
    {
        var specification = new CheckExistingFriendshipSpec(userId, friendId);

        var friendship = await _friendRepository.FirstOrDefaultAsync(specification);

        if (friendship == null)
            throw new KeyNotFoundException("Friendship not found.");

        _friendRepository.Delete(friendship);
        await _friendRepository.SaveChangesAsync();
    }
}

