using VmesteGO.Domain.Enums;

namespace VmesteGO.Dto.Responses;

public class InvitationResponse
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public int ReceiverId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public EventInvitationStatus Status { get; set; }
}