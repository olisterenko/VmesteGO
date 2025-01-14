namespace VmesteGO.Domain.Entities;

public class Notification : BaseEntity<int>
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public required string Text { get; set; } // TODO: required or null!?
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
}