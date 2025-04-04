using VmesteGO.Domain.Enums;

namespace VmesteGO.Domain.Entities;

public class User : BaseEntity<int>
{
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Salt { get; set; } = null!;
    public Role Role { get; set; }
    public string ImageKey { get; set; } = null!;
    public List<FriendRequest> SentFriendRequests { get; set; } = [];
    public List<FriendRequest> ReceivedFriendRequests { get; set; } = [];
    public List<EventInvitation> SentEventInvitations { get; set; } = [];
    public List<EventInvitation> ReceivedEventInvitations { get; set; } = [];
    public List<Notification> Notifications { get; set; } = [];
    public List<Comment> Comments { get; set; } = [];
    public List<UserEvent> UserEvents { get; set; } = [];
}