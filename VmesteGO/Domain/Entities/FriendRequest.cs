using VmesteGO.Domain.Enums;

namespace VmesteGO.Domain.Entities;

public class FriendRequest : BaseEntity<int>
{
    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;
    public int ReceiverId { get; set; }
    public User Receiver { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public FriendRequestStatus Status { get; set; }
}