using VmesteGO.Domain.Enums;

namespace VmesteGO.Domain.Entities;

public class UserEvent : BaseEntity<int>
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    
    public EventStatus EventStatus { get; set; } // TODO: маппинг?
}