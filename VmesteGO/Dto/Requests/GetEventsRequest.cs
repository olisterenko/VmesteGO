using VmesteGO.Domain.Enums;

namespace VmesteGO.Dto.Requests;

public class GetEventsRequest
{
    public int? UserId { get; set; }
    public EventStatus? EventStatus { get; set; }
    public bool IncludePrivate { get; set; }
}