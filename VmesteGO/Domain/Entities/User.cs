namespace VmesteGO.Domain.Entities;

public class User : BaseEntity<int>
{
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Salt { get; set; } = null!;
    public string Role { get; set; } = "User"; // Possible values: "Admin", "User"
    public string ImageUrl { get; set; } = null!;

    public List<Friend> Friends { get; set; } = [];
    public List<FriendRequest> SentFriendRequests { get; set; } = [];
    public List<FriendRequest> ReceivedFriendRequests { get; set; } = [];
    public List<Event> Events { get; set; } = [];
    public List<EventInvitation> SentEventInvitations { get; set; } = [];
    public List<EventInvitation> ReceivedEventInvitations { get; set; } = [];
}