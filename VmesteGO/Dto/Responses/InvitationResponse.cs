using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;

namespace VmesteGO.Dto.Responses;

public class InvitationResponse
{
    public int Id { get; set; }
    public required EventResponse Event { get; set; }
    public required UserResponse Sender { get; set; }
    public required UserResponse Receiver { get; set; }
    public EventInvitationStatus Status { get; set; }
}