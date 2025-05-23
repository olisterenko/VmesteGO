namespace VmesteGO.Dto.Requests;

public class UpdateEventRequest
{
    public required string Title { get; set; }
    public DateTime Dates { get; set; }
    public required string Location { get; set; }
    public required string Description { get; set; }
    public int AgeRestriction { get; set; }
    public decimal Price { get; set; }
    public bool IsPrivate { get; set; } 
    public List<string> EventCategoryNames { get; set; } = [];
    public List<string> EventImages { get; set; } = [];
}