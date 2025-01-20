namespace VmesteGO.Domain.Entities;

public class Event : BaseEntity<int>
{
    public required string Title { get; set; }
    public DateTime Dates { get; set; }
    public required string Location { get; set; }
    public required string Description { get; set; }
    public int AgeRestriction { get; set; }
    public decimal Price { get; set; }
    public bool IsPrivate { get; set; }
    public int? ExternalId { get; set; }
    public int? CreatorId { get; set; }
    public User? Creator { get; set; }
    public List<EventCategory> EventCategories { get; set; } = [];
    public List<EventImage> EventImages { get; set; } = [];
    public List<Comment> Comments { get; set; } = [];
    public List<EventInvitation> EventInvitations { get; set; } = [];
}