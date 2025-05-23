using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;

namespace VmesteGO.Dto.Responses;

public class EventResponse
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public DateTime Dates { get; set; }
    public required string Location { get; set; }
    public required string Description { get; set; }
    public int AgeRestriction { get; set; }
    public decimal Price { get; set; }
    public bool IsPrivate { get; set; }
    public int? ExternalId { get; set; }
    public int? CreatorId { get; set; }
    public required string CreatorUsername { get; set; }
    public List<Category> Categories { get; set; } = [];
    public List<string> Images { get; set; } = [];
    public EventStatus? EventStatus { get; set; }
}