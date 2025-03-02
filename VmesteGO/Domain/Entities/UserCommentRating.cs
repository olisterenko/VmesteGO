namespace VmesteGO.Domain.Entities;

public class UserCommentRating : BaseEntity<int>
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int CommentId { get; set; }
    public Comment Comment { get; set; } = null!;
    public int UserRating { get; set; }
}