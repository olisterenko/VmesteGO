using Moq;
using VmesteGO;
using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;
using VmesteGO.Services;
using VmesteGO.Services.Interfaces;
using VmesteGO.Specifications.EventInvitationSpecs;
using VmesteGO.Specifications.UserEventSpecs;

namespace VmesteGoTests;

[TestClass]
public class EventInvitationServiceTests
{
    private Mock<IRepository<Event>> _eventRepo;
    private Mock<IRepository<User>> _userRepo;
    private Mock<IRepository<EventInvitation>> _invitationRepo;
    private Mock<IRepository<UserEvent>> _userEventRepo;
    private Mock<INotificationService> _notificationService;
    private Mock<IS3StorageService> _s3StorageService;
    private EventInvitationService _service;

    [TestInitialize]
    public void Setup()
    {
        _eventRepo = new Mock<IRepository<Event>>();
        _userRepo = new Mock<IRepository<User>>();
        _invitationRepo = new Mock<IRepository<EventInvitation>>();
        _userEventRepo = new Mock<IRepository<UserEvent>>();
        _notificationService = new Mock<INotificationService>();
        _s3StorageService = new Mock<IS3StorageService>();

        _service = new EventInvitationService(
            _eventRepo.Object,
            _userRepo.Object,
            _invitationRepo.Object,
            _userEventRepo.Object,
            _notificationService.Object,
            _s3StorageService.Object);
    }

    [TestMethod]
    public async Task InviteUserAsync_SendsInvitation_WhenValid()
    {
        // Arrange
        const int eventId = 1;
        const int senderId = 2;
        const int receiverId = 3;

        _eventRepo.Setup(r => r.GetByIdAsync(eventId, CancellationToken.None)).ReturnsAsync(new Event
        {
            Title = "Concert",
            Location = "Moscow",
            Description = "Cool concert"
        });
        _userRepo.Setup(r => r.GetByIdAsync(senderId, CancellationToken.None))
            .ReturnsAsync(new User { Username = "Alice" });
        _userRepo.Setup(r => r.GetByIdAsync(receiverId, CancellationToken.None)).ReturnsAsync(new User());
        _invitationRepo
            .Setup(
                r => r.FirstOrDefaultAsync(It.IsAny<EventInvitationByEventAndReceiverSpec>(), CancellationToken.None))
            .ReturnsAsync((EventInvitation)null);
        _userEventRepo
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<UserEventByUserAndEventSpec>(), CancellationToken.None))
            .ReturnsAsync((UserEvent)null);

        // Act
        await _service.InviteUserAsync(eventId, receiverId, senderId);

        // Assert
        _invitationRepo.Verify(r => r.AddAsync(It.Is<EventInvitation>(i =>
            i.ReceiverId == receiverId && i.SenderId == senderId && i.Status == EventInvitationStatus.Pending
        ), CancellationToken.None), Times.Once);

        _notificationService.Verify(n =>
            n.AddNotificationAsync(receiverId, It.Is<string>(msg => msg.Contains("приглашены")),
                CancellationToken.None), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task InviteUserAsync_Throws_WhenAlreadyInvited()
    {
        // Arrange
        _eventRepo.Setup(r => r.GetByIdAsync(1, CancellationToken.None)).ReturnsAsync(new Event
        {
            Title = "Scorpions",
            Location = "Munich",
            Description = "Legends on stage"
        });
        _userRepo.Setup(r => r.GetByIdAsync(2, CancellationToken.None)).ReturnsAsync(new User());
        _userRepo.Setup(r => r.GetByIdAsync(3, CancellationToken.None)).ReturnsAsync(new User());
        _invitationRepo.Setup(r =>
                r.FirstOrDefaultAsync(It.IsAny<EventInvitationByEventAndReceiverSpec>(), CancellationToken.None))
            .ReturnsAsync(new EventInvitation());

        // Act
        await _service.InviteUserAsync(1, 3, 2);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task InviteUserAsync_Throws_WhenUserAlreadyInEvent()
    {
        // Arrange
        _eventRepo.Setup(r => r.GetByIdAsync(1, CancellationToken.None)).ReturnsAsync(new Event
        {
            Title = "Последнее испытание",
            Location = "КЗ Измайлово",
            Description = "О любви, о вере, о братстве"
        });
        _userRepo.Setup(r => r.GetByIdAsync(2, CancellationToken.None)).ReturnsAsync(new User());
        _userRepo.Setup(r => r.GetByIdAsync(3, CancellationToken.None)).ReturnsAsync(new User());
        _invitationRepo
            .Setup(
                r => r.FirstOrDefaultAsync(It.IsAny<EventInvitationByEventAndReceiverSpec>(), CancellationToken.None))
            .ReturnsAsync((EventInvitation)null);
        _userEventRepo
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<UserEventByUserAndEventSpec>(), CancellationToken.None))
            .ReturnsAsync(new UserEvent());

        // Act
        await _service.InviteUserAsync(1, 3, 2);
    }

    [TestMethod]
    public async Task GetPendingEventInvitationsAsync_ReturnsInvitations()
    {
        // Arrange
        const int userId = 5;
        var invitations = new List<EventInvitation>
        {
            new()
            {
                Id = 1,
                Status = EventInvitationStatus.Pending,
                Event = new Event
                {
                    Title = "Event1",
                    Location = "location1",
                    Description = "description1",
                },
                Sender = new User { Username = "Alice" },
                Receiver = new User { Username = "Bob" }
            }
        };

        _invitationRepo.Setup(r => r.ListAsync(It.IsAny<ReceivedEventInvitationsSpec>(), CancellationToken.None))
            .ReturnsAsync(invitations);
        _s3StorageService.Setup(s => s.GetImageUrl(It.IsAny<string>())).Returns("url");

        // Act
        var result = await _service.GetPendingEventInvitationsAsync(userId);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, result[0].Id);
    }

    [TestMethod]
    public async Task GetSentEventInvitationsAsync_ReturnsInvitations()
    {
        // Arrange
        const int userId = 5;
        var invitations = new List<EventInvitation>
        {
            new()
            {
                Id = 1,
                Status = EventInvitationStatus.Pending,
                Event = new Event
                {
                    Title = "Event 2",
                    Location = "Location 2",
                    Description = "Description 2",
                },
                Sender = new User { Username = "Sender" },
                Receiver = new User { Username = "Receiver" }
            }
        };

        _invitationRepo.Setup(r => r.ListAsync(It.IsAny<SentEventInvitationsSpec>(), CancellationToken.None))
            .ReturnsAsync(invitations);
        _s3StorageService.Setup(s => s.GetImageUrl(It.IsAny<string>())).Returns("url");

        // Act
        var result = await _service.GetSentEventInvitationsAsync(userId);

        // Assert
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task RespondToInvitationAsync_Accepts_AndAddsUserEvent()
    {
        // Arrange
        var invitation = new EventInvitation
        {
            Id = 1,
            EventId = 2,
            ReceiverId = 3,
            SenderId = 4,
            Status = EventInvitationStatus.Pending,
            Receiver = new User { Username = "Bob" },
            Sender = new User { Username = "Alice" },
            Event = new Event
            {
                Title = "A Phantom of the Opera",
                Location = "Austria",
                Description = "some phantom in some opera"
            }
        };

        _invitationRepo.Setup(r => r.FirstAsync(It.IsAny<EventInvitationWithUsersAndEvent>(), CancellationToken.None))
            .ReturnsAsync(invitation);
        _userEventRepo
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<UserEventByUserAndEventSpec>(), CancellationToken.None))
            .ReturnsAsync((UserEvent)null);

        // Act
        await _service.RespondToInvitationAsync(1, EventInvitationStatus.Accepted, 3);

        // Assert
        _userEventRepo.Verify(r => r.Add(It.Is<UserEvent>(ue =>
            ue.EventId == invitation.EventId && ue.UserId == invitation.ReceiverId)), Times.Once);

        _invitationRepo.Verify(r => r.SaveChangesAsync(CancellationToken.None), Times.Once);
        _notificationService.Verify(n =>
                n.AddNotificationAsync(4, It.Is<string>(msg => msg.Contains("получили ответ")), CancellationToken.None),
            Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAccessException))]
    public async Task RespondToInvitationAsync_Throws_IfReceiverMismatch()
    {
        // Arrange
        var invitation = new EventInvitation { Id = 1, ReceiverId = 2 };
        _invitationRepo.Setup(r => r.FirstAsync(It.IsAny<EventInvitationWithUsersAndEvent>(), CancellationToken.None))
            .ReturnsAsync(invitation);

        // Act
        await _service.RespondToInvitationAsync(1, EventInvitationStatus.Accepted, 3);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task RespondToInvitationAsync_Throws_InvalidStatus()
    {
        // Arrange
        var invitation = new EventInvitation { Id = 1, ReceiverId = 3 };
        _invitationRepo.Setup(r => r.FirstAsync(It.IsAny<EventInvitationWithUsersAndEvent>(), CancellationToken.None))
            .ReturnsAsync(invitation);

        // Act
        await _service.RespondToInvitationAsync(1, EventInvitationStatus.Pending, 3);
    }

    [TestMethod]
    public async Task RevokeInvitationAsync_Deletes_AndNotifies()
    {
        // Arrange
        var invitation = new EventInvitation
        {
            Id = 5,
            SenderId = 10,
            ReceiverId = 11,
            Event = new Event
            {
                Title = "Шахматы",
                Location = "МДМ",
                Description = "что-то про любоф"
            },
            Sender = new User { Username = "Alice" }
        };

        _invitationRepo.Setup(r => r.FirstAsync(It.IsAny<EventInvitationWithUsersAndEvent>(), CancellationToken.None))
            .ReturnsAsync(invitation);

        // Act
        await _service.RevokeInvitationAsync(5, 10, CancellationToken.None);

        // Assert
        _invitationRepo.Verify(r => r.DeleteAsync(invitation, CancellationToken.None), Times.Once);
        _notificationService.Verify(n =>
                n.AddNotificationAsync(11, It.Is<string>(msg => msg.Contains("отозвано")), CancellationToken.None),
            Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAccessException))]
    public async Task RevokeInvitationAsync_Throws_WhenUnauthorized()
    {
        // Arrange
        var invitation = new EventInvitation { Id = 1, SenderId = 2 };
        _invitationRepo.Setup(r => r.FirstAsync(It.IsAny<EventInvitationWithUsersAndEvent>(), CancellationToken.None))
            .ReturnsAsync(invitation);

        // Act
        await _service.RevokeInvitationAsync(1, 3, CancellationToken.None);
    }
}