namespace VmesteGO.Dto.Responses;

public class NotificationResponse
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}