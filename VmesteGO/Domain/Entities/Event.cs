namespace VmesteGO.Domain.Entities;

public class Event : BaseEntity<int>
{
    public string Title { get; set; }
    public string Status { get; set; } // TODO: think
    public DateTime Dates { get; set; }
    public string Location { get; set; }
    public ICollection<string> Categories { get; set; } = new List<string>();
    public ICollection<string> Images { get; set; } = new List<string>();
    public string Description { get; set; }
    public int AgeRestriction { get; set; }
    public decimal Price { get; set; }
    public bool IsPrivate { get; set; }
    public List<Comment> Comments { get; set; } = [];
    public List<User> InvitedUsers { get; set; } = [];
    // TODO: external id
    // TODO: think about UserEvent
}