using VmesteGO.Domain.Enums;

namespace VmesteGO.Dto.Requests;

public class ChangeEventStatusRequest
{
    public int UserId { get; set; }
    public int EventId { get; set; }
    public EventStatus NewEventStatus { get; set; }
}