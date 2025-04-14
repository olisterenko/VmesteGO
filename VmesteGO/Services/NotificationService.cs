using AutoMapper;
using VmesteGO.Domain.Entities;
using VmesteGO.Dto.Responses;
using VmesteGO.Services.Interfaces;
using VmesteGO.Specifications.NotificationSpecs;

namespace VmesteGO.Services;

public class NotificationService : INotificationService
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly IMapper _mapper;

    public NotificationService(IRepository<Notification> notificationRepository, IMapper mapper)
    {
        _notificationRepository = notificationRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<NotificationResponse>> GetNotificationsForUserAsync(int userId, bool? isRead = null, CancellationToken cancellationToken = default)
    {
        var spec = new NotificationsForUserSpec(userId, isRead);
        var notifications = await _notificationRepository.ListAsync(spec, cancellationToken);
        return _mapper.Map<IEnumerable<NotificationResponse>>(notifications);
    }

    public async Task MarkAsReadAsync(int notificationId, int userId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        if (notification.UserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to read this notification.");

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _notificationRepository.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default)
    {
        var spec = new NotificationsForUserSpec(userId, isRead: false);
        var unreadNotifications = await _notificationRepository.ListAsync(spec, cancellationToken);

        foreach (var n in unreadNotifications)
        {
            n.IsRead = true;
        }

        await _notificationRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task AddNotificationAsync(int userId, string text, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            Text = text,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);
    }
}
