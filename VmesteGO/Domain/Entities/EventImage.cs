namespace VmesteGO.Domain.Entities;

public class EventImage : BaseEntity<int>
{
    public int EventId { get; set; }
    public Event Event { get; set; } = null!; 
    public string ImageKey { get; set; } = null!;
    public int OrderIndex { get; set; }
}