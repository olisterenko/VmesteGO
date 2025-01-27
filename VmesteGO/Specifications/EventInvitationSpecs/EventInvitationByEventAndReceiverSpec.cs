using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.EventInvitationSpecs;

public sealed class EventInvitationByEventAndReceiverSpec : Specification<EventInvitation>
{
    public EventInvitationByEventAndReceiverSpec(int eventId, int receiverId)
    {
        Query.Where(ei => ei.EventId == eventId && ei.ReceiverId == receiverId);
    }
}