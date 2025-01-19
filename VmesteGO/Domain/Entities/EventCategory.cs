namespace VmesteGO.Domain.Entities;

public class EventCategory : BaseEntity<int>
{
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
