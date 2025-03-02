namespace VmesteGO.Dto.Requests;

public class PostCommentRequest
{
    public int EventId { get; set; }
    public string Text { get; set; } = null!;
}