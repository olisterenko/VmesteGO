using VmesteGO.Dto.Responses;

namespace VmesteGO.Services.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationResponse>> GetNotificationsForUserAsync(int userId, bool? isRead = null, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(int notificationId, int userId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default);
    Task AddNotificationAsync(int userId, string text, CancellationToken cancellationToken = default);
}