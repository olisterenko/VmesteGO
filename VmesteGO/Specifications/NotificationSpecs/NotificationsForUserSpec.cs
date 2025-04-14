using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.NotificationSpecs;

public sealed class NotificationsForUserSpec : Specification<Notification>
{
    public NotificationsForUserSpec(int userId, bool? isRead = null)
    {
        Query
            .Where(n => n.UserId == userId);

        if (isRead is not null)
        {
            Query.Where(n => n.IsRead == isRead);
        }

        Query.OrderByDescending(n => n.CreatedAt);
    }
}