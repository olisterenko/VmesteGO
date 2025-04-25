using AutoMapper;
using Moq;
using VmesteGO;
using VmesteGO.Domain.Entities;
using VmesteGO.Dto.Responses;
using VmesteGO.Services;
using VmesteGO.Specifications.NotificationSpecs;

namespace VmesteGoTests;

[TestClass]
public class NotificationServiceTests
{
    private Mock<IRepository<Notification>> _notificationRepositoryMock;
    private Mock<IMapper> _mapperMock;
    private NotificationService _notificationService;

    [TestInitialize]
    public void Setup()
    {
        _notificationRepositoryMock = new Mock<IRepository<Notification>>();
        _mapperMock = new Mock<IMapper>();
        _notificationService = new NotificationService(_notificationRepositoryMock.Object, _mapperMock.Object);
    }

    [TestMethod]
    public async Task GetNotificationsForUserAsync_ReturnsAllNotifications_WhenIsReadIsNull()
    {
        // Arrange
        const int userId = 1;
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = userId, Text = "Notification 1", IsRead = false, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, UserId = userId, Text = "Notification 2", IsRead = true, CreatedAt = DateTime.UtcNow }
        };

        _notificationRepositoryMock.Setup(repo => repo.ListAsync(
                It.IsAny<NotificationsForUserSpec>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);

        var notificationResponses = notifications.Select(n => new NotificationResponse
        {
            Id = n.Id,
            Text = n.Text,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        });

        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<NotificationResponse>>(notifications))
            .Returns(notificationResponses);

        // Act
        var result = await _notificationService.GetNotificationsForUserAsync(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count());
        _notificationRepositoryMock.Verify(repo => repo.ListAsync(
            It.IsAny<NotificationsForUserSpec>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<IEnumerable<NotificationResponse>>(notifications), Times.Once);
    }

    [TestMethod]
    public async Task GetNotificationsForUserAsync_ReturnsUnreadNotifications_WhenIsReadIsFalse()
    {
        // Arrange
        const int userId = 1;
        const bool isRead = false;
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = userId, Text = "Notification 1", IsRead = false, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, UserId = userId, Text = "Notification 2", IsRead = false, CreatedAt = DateTime.UtcNow }
        };

        _notificationRepositoryMock.Setup(repo => repo.ListAsync(
                It.IsAny<NotificationsForUserSpec>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);

        var notificationResponses = notifications.Select(n => new NotificationResponse
        {
            Id = n.Id,
            Text = n.Text,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        });

        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<NotificationResponse>>(notifications))
            .Returns(notificationResponses);

        // Act
        var result = await _notificationService.GetNotificationsForUserAsync(userId, isRead);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count());
        _notificationRepositoryMock.Verify(repo => repo.ListAsync(
            It.IsAny<NotificationsForUserSpec>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<IEnumerable<NotificationResponse>>(notifications), Times.Once);
    }

    [TestMethod]
    public async Task GetNotificationsForUserAsync_ReturnsReadNotifications_WhenIsReadIsTrue()
    {
        // Arrange
        const int userId = 1;
        const bool isRead = true;
        var notifications = new List<Notification>
        {
            new() { Id = 3, UserId = userId, Text = "Notification 3", IsRead = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 4, UserId = userId, Text = "Notification 4", IsRead = true, CreatedAt = DateTime.UtcNow }
        };

        _notificationRepositoryMock.Setup(repo => repo.ListAsync(
                It.IsAny<NotificationsForUserSpec>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);

        var notificationResponses = notifications.Select(n => new NotificationResponse
        {
            Id = n.Id,
            Text = n.Text,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        });

        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<NotificationResponse>>(notifications))
            .Returns(notificationResponses);

        // Act
        var result = await _notificationService.GetNotificationsForUserAsync(userId, isRead);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count());
        _notificationRepositoryMock.Verify(repo => repo.ListAsync(
            It.IsAny<NotificationsForUserSpec>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<IEnumerable<NotificationResponse>>(notifications), Times.Once);
    }

    [TestMethod]
    public async Task GetNotificationsForUserAsync_ReturnsEmpty_WhenNoNotifications()
    {
        // Arrange
        const int userId = 1;
        var notifications = new List<Notification>();

        _notificationRepositoryMock.Setup(repo => repo.ListAsync(
                It.IsAny<NotificationsForUserSpec>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);

        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<NotificationResponse>>(notifications))
            .Returns([]);

        // Act
        var result = await _notificationService.GetNotificationsForUserAsync(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
        _notificationRepositoryMock.Verify(repo => repo.ListAsync(
            It.IsAny<NotificationsForUserSpec>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<IEnumerable<NotificationResponse>>(notifications), Times.Once);
    }


    [TestMethod]
    public async Task MarkAsReadAsync_MarksNotificationAsRead()
    {
        // Arrange
        const int userId = 1;
        const int notificationId = 10;
        var notification = new Notification
        {
            Id = notificationId,
            UserId = userId,
            Text = "Unread Notification",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _notificationRepositoryMock.Setup(repo => repo.GetByIdAsync(notificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);
        _notificationRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _notificationService.MarkAsReadAsync(notificationId, userId);

        // Assert
        Assert.IsTrue(notification.IsRead, "Notification should be marked as read.");
        _notificationRepositoryMock.Verify(repo => repo.GetByIdAsync(notificationId, It.IsAny<CancellationToken>()),
            Times.Once);
        _notificationRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAccessException))]
    public async Task MarkAsReadAsync_ThrowsException_WhenNotificationDoesNotBelongToUser()
    {
        // Arrange
        const int userId = 1;
        const int notificationId = 12;
        var notification = new Notification
        {
            Id = notificationId,
            UserId = 2,
            Text = "Another User's Notification",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _notificationRepositoryMock.Setup(repo => repo.GetByIdAsync(notificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        // Act
        await _notificationService.MarkAsReadAsync(notificationId, userId);
    }

    [TestMethod]
    public async Task MarkAllAsReadAsync_MarksAllUnreadNotificationsAsRead()
    {
        // Arrange
        const int userId = 1;
        const bool isRead = false;
        var unreadNotifications = new List<Notification>
        {
            new()
            {
                Id = 20, UserId = userId, Text = "Unread Notification 1", IsRead = false, CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 21, UserId = userId, Text = "Unread Notification 2", IsRead = false, CreatedAt = DateTime.UtcNow
            }
        };

        _notificationRepositoryMock.Setup(repo => repo.ListAsync(
                It.IsAny<NotificationsForUserSpec>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(unreadNotifications);
        _notificationRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // Act
        await _notificationService.MarkAllAsReadAsync(userId);

        // Assert
        foreach (var notification in unreadNotifications)
        {
            Assert.IsTrue(notification.IsRead, $"Notification ID {notification.Id} should be marked as read.");
        }

        _notificationRepositoryMock.Verify(repo => repo.ListAsync(
            It.IsAny<NotificationsForUserSpec>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _notificationRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [TestMethod]
    public async Task AddNotificationAsync_AddsNotification()
    {
        // Arrange
        const int userId = 1;
        const string text = "New Notification";
        var cancellationToken = new CancellationToken();

        Notification addedNotification = null;

        _notificationRepositoryMock.Setup(repo => repo.AddAsync(
                It.IsAny<Notification>(),
                cancellationToken))
            .Callback<Notification, CancellationToken>((n, ct) => addedNotification = n)
            .ReturnsAsync((Notification n, CancellationToken ct) => n);

        // Act
        await _notificationService.AddNotificationAsync(userId, text, cancellationToken);

        // Assert
        _notificationRepositoryMock.Verify(repo => repo.AddAsync(
            It.Is<Notification>(n => n.UserId == userId && n.Text == text && !n.IsRead),
            cancellationToken), Times.Once);
        Assert.IsNotNull(addedNotification);
        Assert.AreEqual(userId, addedNotification.UserId);
        Assert.AreEqual(text, addedNotification.Text);
        Assert.IsFalse(addedNotification.IsRead);
        Assert.IsTrue((DateTime.UtcNow - addedNotification.CreatedAt).TotalSeconds < 5);
    }
}