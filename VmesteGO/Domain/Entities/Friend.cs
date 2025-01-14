namespace VmesteGO.Domain.Entities;

public class Friend : BaseEntity<int>
{
    public int UserId { get; set; }
    public int FriendUserId { get; set; }
    public User User { get; set; } = null!;
    public User FriendUser { get; set; } = null!;
}