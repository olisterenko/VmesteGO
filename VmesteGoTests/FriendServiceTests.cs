using AutoMapper;
using Moq;
using VmesteGO;
using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;
using VmesteGO.Services;
using VmesteGO.Services.Interfaces;
using VmesteGO.Specifications.FriendRequestSpecs;
using VmesteGO.Specifications.UserEventSpecs;

namespace VmesteGoTests;

[TestClass]
public class FriendServiceTests
{
    private Mock<IRepository<FriendRequest>> _friendRequestRepositoryMock;
    private Mock<IRepository<User>> _userRepositoryMock;
    private Mock<IRepository<UserEvent>> _userEventRepositoryMock;
    private Mock<IMapper> _mapperMock;
    private Mock<IS3StorageService> _s3ServiceMock;
    private Mock<INotificationService> _notificationServiceMock;
    private FriendService _friendService;

    [TestInitialize]
    public void Setup()
    {
        _friendRequestRepositoryMock = new Mock<IRepository<FriendRequest>>();
        _userRepositoryMock = new Mock<IRepository<User>>();
        _userEventRepositoryMock = new Mock<IRepository<UserEvent>>();
        _mapperMock = new Mock<IMapper>();
        _s3ServiceMock = new Mock<IS3StorageService>();
        _notificationServiceMock = new Mock<INotificationService>();

        _friendService = new FriendService(
            _friendRequestRepositoryMock.Object,
            _userRepositoryMock.Object,
            _userEventRepositoryMock.Object,
            _mapperMock.Object,
            _s3ServiceMock.Object,
            _notificationServiceMock.Object);
    }


    [TestMethod]
    public async Task SendFriendRequestAsync_Success()
    {
        // Arrange
        const int senderId = 1;
        const int receiverId = 2;

        var sender = new User { Id = senderId, Username = "SenderUser" };
        var receiver = new User { Id = receiverId, Username = "ReceiverUser" };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(senderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sender);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(receiverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(receiver);

        _friendRequestRepositoryMock.Setup(repo => repo.FirstOrDefaultAsync(
                It.IsAny<CheckExistingFriendRequestSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FriendRequest)null);
        _friendRequestRepositoryMock.Setup(repo => repo.AddAsync(
                It.IsAny<FriendRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FriendRequest fr, CancellationToken ct) => fr);
        _friendRequestRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _notificationServiceMock.Setup(n => n.AddNotificationAsync(
                receiver.Id,
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _friendService.SendFriendRequestAsync(senderId, receiverId);

        // Assert
        _friendRequestRepositoryMock.Verify(repo => repo.AddAsync(It.Is<FriendRequest>(fr =>
            fr.SenderId == senderId &&
            fr.ReceiverId == receiverId &&
            fr.Status == FriendRequestStatus.Pending), It.IsAny<CancellationToken>()), Times.Once);
        _friendRequestRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notificationServiceMock.Verify(n => n.AddNotificationAsync(
            receiver.Id,
            $"Вы получили запрос в друзья от {sender.Username}.", It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task SendFriendRequestAsync_SenderIsReceiver_ThrowsException()
    {
        // Arrange
        const int userId = 1;

        // Act
        await _friendService.SendFriendRequestAsync(userId, userId);
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public async Task SendFriendRequestAsync_SenderOrReceiverNotFound_ThrowsException()
    {
        // Arrange
        const int senderId = 1;
        const int receiverId = 2;

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(senderId, It.IsAny<CancellationToken>()))!
            .ReturnsAsync((User)null);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(receiverId, It.IsAny<CancellationToken>()))!
            .ReturnsAsync((User)null);

        // Act
        await _friendService.SendFriendRequestAsync(senderId, receiverId);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task SendFriendRequestAsync_ExistingFriendship_ThrowsException()
    {
        // Arrange
        const int senderId = 1;
        const int receiverId = 2;

        var sender = new User { Id = senderId, Username = "SenderUser" };
        var receiver = new User { Id = receiverId, Username = "ReceiverUser" };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(senderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sender);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(receiverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(receiver);

        _friendRequestRepositoryMock.Setup(repo => repo.FirstOrDefaultAsync(
                It.IsAny<CheckExistingFriendRequestSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FriendRequest());

        // Act
        await _friendService.SendFriendRequestAsync(senderId, receiverId);
    }

    [TestMethod]
    public async Task AcceptFriendRequestAsync_Success()
    {
        // Arrange
        const int receiverId = 2;
        const int requestId = 3;

        var friendRequest = new FriendRequest
        {
            Id = requestId,
            SenderId = 1,
            ReceiverId = receiverId,
            Status = FriendRequestStatus.Pending,
            Sender = new User { Id = 1, Username = "SenderUser" },
            Receiver = new User { Id = receiverId, Username = "ReceiverUser" }
        };

        _friendRequestRepositoryMock.Setup(repo => repo.FirstAsync(
                It.IsAny<FriendRequestWithUsersSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(friendRequest);
        _friendRequestRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _notificationServiceMock.Setup(n => n.AddNotificationAsync(
                friendRequest.SenderId,
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _friendService.AcceptFriendRequestAsync(receiverId, requestId);

        // Assert
        Assert.AreEqual(FriendRequestStatus.Accepted, friendRequest.Status);
        _friendRequestRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notificationServiceMock.Verify(n => n.AddNotificationAsync(
                friendRequest.SenderId,
                $"Ваш запрос в друзья {friendRequest.Receiver.Username} был принят.", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAccessException))]
    public async Task AcceptFriendRequestAsync_NotReceiver_ThrowsException()
    {
        // Arrange
        const int receiverId = 2;
        const int wrongReceiverId = 3;
        const int requestId = 4;

        var friendRequest = new FriendRequest
        {
            Id = requestId,
            SenderId = 1,
            ReceiverId = receiverId,
            Status = FriendRequestStatus.Pending,
            Sender = new User { Id = 1, Username = "SenderUser" },
            Receiver = new User { Id = receiverId, Username = "ReceiverUser" }
        };

        _friendRequestRepositoryMock.Setup(repo => repo.FirstAsync(
                It.IsAny<FriendRequestWithUsersSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(friendRequest);

        // Act
        await _friendService.AcceptFriendRequestAsync(wrongReceiverId, requestId);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task AcceptFriendRequestAsync_InvalidStatus_ThrowsException()
    {
        // Arrange
        const int receiverId = 2;
        const int requestId = 3;

        var friendRequest = new FriendRequest
        {
            Id = requestId,
            SenderId = 1,
            ReceiverId = receiverId,
            Status = FriendRequestStatus.Accepted, // Already accepted
            Sender = new User { Id = 1, Username = "SenderUser" },
            Receiver = new User { Id = receiverId, Username = "ReceiverUser" }
        };

        _friendRequestRepositoryMock.Setup(repo => repo.FirstAsync(
                It.IsAny<FriendRequestWithUsersSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(friendRequest);

        // Act
        await _friendService.AcceptFriendRequestAsync(receiverId, requestId);
    }

    [TestMethod]
    public async Task RejectFriendRequestAsync_Success()
    {
        // Arrange
        const int receiverId = 2;
        const int requestId = 3;

        var friendRequest = new FriendRequest
        {
            Id = requestId,
            SenderId = 1,
            ReceiverId = receiverId,
            Status = FriendRequestStatus.Pending,
            Sender = new User { Id = 1, Username = "SenderUser" },
            Receiver = new User { Id = receiverId, Username = "ReceiverUser" }
        };

        _friendRequestRepositoryMock.Setup(repo => repo.FirstAsync(
                It.IsAny<FriendRequestWithUsersSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(friendRequest);
        _friendRequestRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _notificationServiceMock.Setup(n => n.AddNotificationAsync(
                friendRequest.SenderId,
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _friendService.RejectFriendRequestAsync(receiverId, requestId);

        // Assert
        Assert.AreEqual(FriendRequestStatus.Rejected, friendRequest.Status);
        _friendRequestRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notificationServiceMock.Verify(n => n.AddNotificationAsync(
                friendRequest.SenderId,
                $"Ваш запрос в друзья {friendRequest.Receiver.Username} был отклонен.", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAccessException))]
    public async Task RejectFriendRequestAsync_NotReceiver_ThrowsException()
    {
        // Arrange
        const int receiverId = 2;
        const int wrongReceiverId = 3;
        const int requestId = 4;

        var friendRequest = new FriendRequest
        {
            Id = requestId,
            SenderId = 1,
            ReceiverId = receiverId,
            Status = FriendRequestStatus.Pending,
            Sender = new User { Id = 1, Username = "SenderUser" },
            Receiver = new User { Id = receiverId, Username = "ReceiverUser" }
        };

        _friendRequestRepositoryMock.Setup(repo => repo.FirstAsync(
                It.IsAny<FriendRequestWithUsersSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(friendRequest);

        // Act
        await _friendService.RejectFriendRequestAsync(wrongReceiverId, requestId);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task RejectFriendRequestAsync_InvalidStatus_ThrowsException()
    {
        // Arrange
        const int receiverId = 2;
        const int requestId = 3;

        var friendRequest = new FriendRequest
        {
            Id = requestId,
            SenderId = 1,
            ReceiverId = receiverId,
            Status = FriendRequestStatus.Accepted, // Already accepted
            Sender = new User { Id = 1, Username = "SenderUser" },
            Receiver = new User { Id = receiverId, Username = "ReceiverUser" }
        };

        _friendRequestRepositoryMock.Setup(repo => repo.FirstAsync(
                It.IsAny<FriendRequestWithUsersSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(friendRequest);

        // Act
        await _friendService.RejectFriendRequestAsync(receiverId, requestId);
    }


    [TestMethod]
    public async Task GetFriendsAsync_ReturnsFriends()
    {
        // Arrange
        const int userId = 1;
        var friends = new List<FriendRequest>
        {
            new()
            {
                Id = 1,
                SenderId = userId,
                ReceiverId = 2,
                Sender = new User { Id = userId, Username = "User1", ImageKey = "image1" },
                Receiver = new User { Id = 2, Username = "User2", ImageKey = "image2" }
            },
            new()
            {
                Id = 2,
                SenderId = 3,
                ReceiverId = userId,
                Sender = new User { Id = 3, Username = "User3", ImageKey = "image3" },
                Receiver = new User { Id = userId, Username = "User1", ImageKey = "image1" }
            }
        };

        _friendRequestRepositoryMock.Setup(repo => repo.ListAsync(
                It.IsAny<FriendsOfUserSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(friends);

        _s3ServiceMock.Setup(s3 => s3.GetImageUrl(It.IsAny<string>()))
            .Returns<string>(key => $"https://s3.amazonaws.com/bucket/{key}");

        // Act
        var result = await _friendService.GetFriendsAsync(userId);

        // Assert
        var friendResponses = result.ToList();
        var firstFriend = friendResponses.First();
        Assert.AreEqual(2, friendResponses.Count);
        Assert.AreEqual(1, firstFriend.Id);
        Assert.AreEqual(2, firstFriend.FriendUserId);
        Assert.AreEqual("User2", firstFriend.FriendUsername);
        Assert.AreEqual("https://s3.amazonaws.com/bucket/image2", firstFriend.FriendImageUrl);
    }

    [TestMethod]
    public async Task GetFriendsAsync_NoFriends_ReturnsEmpty()
    {
        // Arrange
        const int userId = 1;

        _friendRequestRepositoryMock.Setup(repo => repo.ListAsync(
                It.IsAny<FriendsOfUserSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _friendService.GetFriendsAsync(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }


    [TestMethod]
    public async Task GetPendingFriendRequestsAsync_ReturnsPendingRequests()
    {
        // Arrange
        const int userId = 1;
        var pendingRequests = new List<FriendRequest>
        {
            new()
            {
                Id = 1,
                SenderId = 2,
                ReceiverId = userId,
                Sender = new User { Id = 2, Username = "User2", ImageKey = "image2" },
                Receiver = new User { Id = userId, Username = "User1", ImageKey = "image1" },
                Status = FriendRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            }
        };

        _friendRequestRepositoryMock.Setup(repo => repo.ListAsync(
                It.IsAny<PendingFriendRequestsSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingRequests);

        _s3ServiceMock.Setup(s3 => s3.GetImageUrl(It.IsAny<string>()))
            .Returns<string>(key => $"https://s3.amazonaws.com/bucket/{key}");

        // Act
        var result = await _friendService.GetPendingFriendRequestsAsync(userId);

        // Assert
        var friendRequestResponses = result.ToList();
        var request = friendRequestResponses.First();
        Assert.AreEqual(1, friendRequestResponses.Count);
        Assert.AreEqual(1, request.Id);
        Assert.AreEqual(2, request.Sender.Id);
        Assert.AreEqual("User2", request.Sender.Username);
        Assert.AreEqual("https://s3.amazonaws.com/bucket/image2", request.Sender.ImageUrl);
        Assert.AreEqual(userId, request.Receiver.Id);
        Assert.AreEqual("User1", request.Receiver.Username);
        Assert.AreEqual("https://s3.amazonaws.com/bucket/image1", request.Receiver.ImageUrl);
        Assert.AreEqual(FriendRequestStatus.Pending, request.Status);
    }

    [TestMethod]
    public async Task GetPendingFriendRequestsAsync_NoPendingRequests_ReturnsEmpty()
    {
        // Arrange
        const int userId = 1;

        _friendRequestRepositoryMock.Setup(repo => repo.ListAsync(
                It.IsAny<PendingFriendRequestsSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _friendService.GetPendingFriendRequestsAsync(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }


    [TestMethod]
    public async Task GetSentFriendRequestsAsync_ReturnsSentRequests()
    {
        // Arrange
        const int userId = 1;
        var sentRequests = new List<FriendRequest>
        {
            new()
            {
                Id = 1,
                SenderId = userId,
                ReceiverId = 2,
                Sender = new User { Id = userId, Username = "User1", ImageKey = "image1" },
                Receiver = new User { Id = 2, Username = "User2", ImageKey = "image2" },
                Status = FriendRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            }
        };

        _friendRequestRepositoryMock.Setup(repo => repo.ListAsync(
                It.IsAny<SentFriendRequestsSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sentRequests);
        _s3ServiceMock.Setup(s3 => s3.GetImageUrl(It.IsAny<string>()))
            .Returns<string>(key => $"https://s3.amazonaws.com/bucket/{key}");

        // Act
        var result = await _friendService.GetSentFriendRequestsAsync(userId);

        // Assert
        var friendRequestResponses = result.ToList();
        var request = friendRequestResponses.First();
        Assert.AreEqual(1, friendRequestResponses.Count);
        Assert.AreEqual(1, request.Id);
        Assert.AreEqual(userId, request.Sender.Id);
        Assert.AreEqual("User1", request.Sender.Username);
        Assert.AreEqual("https://s3.amazonaws.com/bucket/image1", request.Sender.ImageUrl);
        Assert.AreEqual(2, request.Receiver.Id);
        Assert.AreEqual("User2", request.Receiver.Username);
        Assert.AreEqual("https://s3.amazonaws.com/bucket/image2", request.Receiver.ImageUrl);
        Assert.AreEqual(FriendRequestStatus.Pending, request.Status);
    }

    [TestMethod]
    public async Task GetSentFriendRequestsAsync_NoSentRequests_ReturnsEmpty()
    {
        // Arrange
        const int userId = 1;

        _friendRequestRepositoryMock.Setup(repo => repo.ListAsync(
                It.IsAny<SentFriendRequestsSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _friendService.GetSentFriendRequestsAsync(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }


    [TestMethod]
    public async Task GetFriendRequest_Found_ReturnsFriendRequestResponse()
    {
        // Arrange
        const int senderId = 1;
        const int receiverId = 2;

        var friendRequest = new FriendRequest
        {
            Id = 1,
            SenderId = senderId,
            ReceiverId = receiverId,
            Sender = new User { Id = senderId, Username = "User1", ImageKey = "image1" },
            Receiver = new User { Id = receiverId, Username = "User2", ImageKey = "image2" },
            CreatedAt = DateTime.UtcNow,
            Status = FriendRequestStatus.Pending
        };

        _friendRequestRepositoryMock.Setup(repo => repo.SingleOrDefaultAsync(
                It.IsAny<GetFriendRequestForUsersSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(friendRequest);

        _s3ServiceMock.Setup(s3 => s3.GetImageUrl(It.IsAny<string>()))
            .Returns<string>(key => $"https://s3.amazonaws.com/bucket/{key}");

        // Act
        var result = await _friendService.GetFriendRequest(senderId, receiverId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(friendRequest.Id, result.Id);
        Assert.AreEqual(senderId, result.Sender.Id);
        Assert.AreEqual("User1", result.Sender.Username);
        Assert.AreEqual("https://s3.amazonaws.com/bucket/image1", result.Sender.ImageUrl);
        Assert.AreEqual(receiverId, result.Receiver.Id);
        Assert.AreEqual("User2", result.Receiver.Username);
        Assert.AreEqual("https://s3.amazonaws.com/bucket/image2", result.Receiver.ImageUrl);
        Assert.AreEqual(friendRequest.CreatedAt, result.CreatedAt);
        Assert.AreEqual(friendRequest.Status, result.Status);
    }

    [TestMethod]
    public async Task GetFriendRequest_NotFound_ReturnsNull()
    {
        // Arrange
        const int senderId = 1;
        const int receiverId = 2;

        _friendRequestRepositoryMock.Setup(repo => repo.SingleOrDefaultAsync(
                It.IsAny<GetFriendRequestForUsersSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FriendRequest)null);

        // Act
        var result = await _friendService.GetFriendRequest(senderId, receiverId);

        // Assert
        Assert.IsNull(result);
    }


    [TestMethod]
    public async Task RemoveFriendAsync_UserIsSender_SetsStatusPending()
    {
        // Arrange
        const int userId = 1;
        const int friendId = 2;

        var user = new User { Id = userId, Username = "User1" };
        var friendship = new FriendRequest
        {
            SenderId = userId,
            ReceiverId = friendId,
            Status = FriendRequestStatus.Accepted,
            Sender = user,
            Receiver = new User { Id = friendId, Username = "User2" }
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _friendRequestRepositoryMock.Setup(repo => repo.FirstAsync(
                It.IsAny<CheckExistingFriendshipSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(friendship);
        _friendRequestRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _notificationServiceMock.Setup(n => n.AddNotificationAsync(
                friendId,
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _friendService.RemoveFriendAsync(userId, friendId);

        // Assert
        Assert.AreEqual(FriendRequestStatus.Pending, friendship.Status);
        _friendRequestRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notificationServiceMock.Verify(n => n.AddNotificationAsync(
            friendId,
            $"Вы были удалены из друзей {user.Username}.", It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task RemoveFriendAsync_UserIsReceiver_SetsStatusRejected()
    {
        // Arrange
        const int userId = 1;
        const int friendId = 2;

        var user = new User { Id = userId, Username = "User1" };
        var friendship = new FriendRequest
        {
            SenderId = friendId,
            ReceiverId = userId,
            Status = FriendRequestStatus.Accepted,
            Sender = new User { Id = friendId, Username = "User2" },
            Receiver = user
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _friendRequestRepositoryMock.Setup(repo => repo.FirstAsync(
                It.IsAny<CheckExistingFriendshipSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(friendship);
        _friendRequestRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _notificationServiceMock.Setup(n => n.AddNotificationAsync(
                friendId,
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _friendService.RemoveFriendAsync(userId, friendId);

        // Assert
        Assert.AreEqual(FriendRequestStatus.Rejected, friendship.Status);
        _friendRequestRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notificationServiceMock.Verify(n => n.AddNotificationAsync(
            friendId,
            $"Вы были удалены из друзей {user.Username}.", It.IsAny<CancellationToken>()), Times.Once);
    }


    [TestMethod]
    public async Task RevokeFriendRequestAsync_Success()
    {
        // Arrange
        const int requestId = 3;
        const int userId = 1;
        var cancellationToken = CancellationToken.None;

        var friendRequest = new FriendRequest
        {
            Id = requestId,
            SenderId = userId,
            ReceiverId = 2,
            Sender = new User { Id = userId, Username = "User1" },
            Receiver = new User { Id = 2, Username = "User2" },
            Status = FriendRequestStatus.Pending
        };

        _friendRequestRepositoryMock.Setup(repo => repo.FirstAsync(
                It.IsAny<FriendRequestWithUsersSpec>(), cancellationToken))
            .ReturnsAsync(friendRequest);
        _friendRequestRepositoryMock.Setup(repo => repo.DeleteAsync(friendRequest, cancellationToken))
            .Returns(Task.CompletedTask);

        _notificationServiceMock.Setup(n => n.AddNotificationAsync(
                friendRequest.ReceiverId,
                It.IsAny<string>(),
                cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _friendService.RevokeFriendRequestAsync(requestId, userId, cancellationToken);

        // Assert
        _friendRequestRepositoryMock.Verify(repo => repo.DeleteAsync(friendRequest, cancellationToken), Times.Once);
        _notificationServiceMock.Verify(n => n.AddNotificationAsync(
            friendRequest.ReceiverId,
            $"Запрос в друзья от {friendRequest.Sender.Username} был отозван.",
            cancellationToken), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAccessException))]
    public async Task RevokeFriendRequestAsync_NotSender_ThrowsException()
    {
        // Arrange
        const int requestId = 4;
        const int userId = 1;
        var cancellationToken = CancellationToken.None;

        var friendRequest = new FriendRequest
        {
            Id = requestId,
            SenderId = 2,
            ReceiverId = 3,
            Sender = new User { Id = 2, Username = "User2" },
            Receiver = new User { Id = 3, Username = "User3" },
            Status = FriendRequestStatus.Pending
        };

        _friendRequestRepositoryMock.Setup(repo => repo.FirstAsync(
                It.IsAny<FriendRequestWithUsersSpec>(), cancellationToken))
            .ReturnsAsync(friendRequest);

        // Act
        await _friendService.RevokeFriendRequestAsync(requestId, userId, cancellationToken);
    }


    [TestMethod]
    public async Task GetFriendsEventsAsync_ReturnsEvents()
    {
        // Arrange
        const int userId = 1;
        const string user1 = nameof(user1);
        const string user2 = nameof(user2);
        const string user3 = nameof(user3);
        const string imageKey1 = nameof(imageKey1);
        const string imageKey2 = nameof(imageKey2);
        const string imageKey3 = nameof(imageKey3);

        var event1 = new Event
        {
            Id = 10,
            Title = "Event1",
            Description = "Desc1",
            Location = "Loc1"
        };
        var event2 = new Event
        {
            Id = 20,
            Title = "Event2",
            Description = "Desc2",
            Location = "Loc2"
        };

        var friends = new List<FriendRequest>
        {
            new()
            {
                Id = 1,
                SenderId = userId,
                ReceiverId = 2,
                Sender = new User { Id = userId, Username = user1, ImageKey = imageKey1 },
                Receiver = new User { Id = 2, Username = user2, ImageKey = imageKey2 }
            },
            new()
            {
                Id = 2,
                SenderId = 3,
                ReceiverId = userId,
                Sender = new User { Id = 3, Username = user3, ImageKey = imageKey3 },
                Receiver = new User { Id = userId, Username = user1, ImageKey = imageKey1 }
            }
        };

        var userEvents = new List<UserEvent>
        {
            new()
            {
                UserId = 2,
                User = new User { Id = 2, Username = user2, ImageKey = imageKey2 },
                Event = event1,
                EventStatus = EventStatus.Going
            },
            new()
            {
                UserId = 3,
                User = new User { Id = 3, Username = user3, ImageKey = imageKey3 },
                Event = event1,
                EventStatus = EventStatus.WantToGo
            },
            new()
            {
                UserId = 2,
                User = new User { Id = 2, Username = user2, ImageKey = imageKey2 },
                Event = event2,
                EventStatus = EventStatus.Going
            }
        };

        _friendRequestRepositoryMock.Setup(repo => repo.ListAsync(
                It.IsAny<FriendsOfUserSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(friends);

        _userEventRepositoryMock.Setup(repo => repo.ListAsync(
                It.IsAny<FriendsEventsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userEvents);

        _s3ServiceMock.Setup(s3 => s3.GetImageUrl(It.IsAny<string>()))
            .Returns<string>(key => $"https://s3.amazonaws.com/bucket/{key}");

        // Act
        var result = await _friendService.GetFriendsEventsAsync(userId);

        // Assert
        var friendEventResponses = result.ToList();
        Assert.AreEqual(2, friendEventResponses.Count);

        var friendEventResponse1 = friendEventResponses.FirstOrDefault(e => e.EventResponse.Id == 10);
        Assert.IsNotNull(friendEventResponse1);
        Assert.AreEqual("Event1", friendEventResponse1.EventResponse.Title);
        Assert.AreEqual(2, friendEventResponse1.Friends.Count());

        var friendEventResponse2 = friendEventResponses.FirstOrDefault(e => e.EventResponse.Id == 20);
        Assert.IsNotNull(friendEventResponse2);
        Assert.AreEqual("Event2", friendEventResponse2.EventResponse.Title);
        Assert.AreEqual(1, friendEventResponse2.Friends.Count());
    }

    [TestMethod]
    public async Task GetFriendsEventsAsync_NoFriends_ReturnsEmpty()
    {
        // Arrange
        const int userId = 1;

        _friendRequestRepositoryMock.Setup(repo => repo.ListAsync(
                It.IsAny<FriendsOfUserSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _friendService.GetFriendsEventsAsync(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }


    [TestMethod]
    public async Task GetFriendEventByIdAsync_ReturnsFriendEventStatusResponses()
    {
        // Arrange
        const int userId = 1;
        const int eventId = 10;

        var friends = new List<FriendRequest>
        {
            new()
            {
                Id = 1,
                SenderId = userId,
                ReceiverId = 2,
                Sender = new User { Id = userId, Username = "User1", ImageKey = "image1" },
                Receiver = new User { Id = 2, Username = "User2", ImageKey = "image2" }
            },
            new()
            {
                Id = 2,
                SenderId = 3,
                ReceiverId = userId,
                Sender = new User { Id = 3, Username = "User3", ImageKey = "image3" },
                Receiver = new User { Id = userId, Username = "User1", ImageKey = "image1" }
            }
        };

        var userEvents = new List<UserEvent>
        {
            new()
            {
                UserId = 2,
                User = new User { Id = 2, Username = "User2", ImageKey = "image2" },
                EventId = eventId,
                Event = new Event
                {
                    Id = eventId,
                    Title = "Event1",
                    Description = "Desc1",
                    Location = "Loc1"
                },
                EventStatus = EventStatus.Going
            }
        };

        _friendRequestRepositoryMock.Setup(repo => repo.ListAsync(
                It.IsAny<FriendsOfUserSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(friends);

        _userEventRepositoryMock.Setup(repo => repo.ListAsync(
                It.IsAny<FriendsSpecificEventSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userEvents);

        _s3ServiceMock.Setup(s3 => s3.GetImageUrl(It.IsAny<string>()))
            .Returns<string>(key => $"https://s3.amazonaws.com/bucket/{key}");

        // Act
        var result = await _friendService.GetFriendEventByIdAsync(userId, eventId);

        // Assert
        Assert.AreEqual(2, result.Count);

        var user2Status = result.FirstOrDefault(r => r.Friend.Id == 2);
        Assert.IsNotNull(user2Status);
        Assert.AreEqual(EventStatus.Going, user2Status.EventStatus);

        var user3Status = result.FirstOrDefault(r => r.Friend.Id == 3);
        Assert.IsNotNull(user3Status);
        Assert.IsNull(user3Status.EventStatus);
    }

    [TestMethod]
    public async Task GetFriendEventByIdAsync_NoFriends_ReturnsEmpty()
    {
        // Arrange
        const int userId = 1;
        const int eventId = 10;

        _friendRequestRepositoryMock.Setup(repo => repo.ListAsync(
                It.IsAny<FriendsOfUserSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _friendService.GetFriendEventByIdAsync(userId, eventId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }
}