using VmesteGO.Domain.Enums;

namespace VmesteGO.Domain.Entities;

public class UserEvent : BaseEntity<int>
{
    public int UserId { get; set; }
    public User User { get; set; } = default!;
    
    public int EventId { get; set; }
    public Event Event { get; set; } = default!;
    
    public int UserRating { get; set; }
    public EventStatus EventStatus { get; set; } // TODO: маппинг?
}