namespace VmesteGO.Domain.Entities;

public class Comment : BaseEntity<int>
{
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public required string Text { get; set; }
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<UserCommentRating> UserCommentRatings { get; set; } = [];
}