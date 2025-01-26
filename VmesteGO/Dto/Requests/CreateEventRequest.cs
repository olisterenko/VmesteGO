namespace VmesteGO.Dto.Requests;

public class CreateEventRequest
{
    public required string Title { get; set; }
    public DateTime Dates { get; set; }
    public required string Location { get; set; }
    public required string Description { get; set; }
    public int AgeRestriction { get; set; }
    public decimal Price { get; set; }
    public bool IsPrivate { get; set; }
    public List<int> EventCategoryIds { get; set; } = [];
    public List<string> EventImages { get; set; } = []; 
    public int? ExternalId { get; set; }
}

