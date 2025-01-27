using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.EventInvitationSpecs;

public sealed class EventInvitationsForReceiverSpec : Specification<EventInvitation>
{
    public EventInvitationsForReceiverSpec(int receiverId)
    {
        Query.Where(ei => ei.ReceiverId == receiverId)
            .Include(ei => ei.Event)
            .Include(ei => ei.Sender);
    }
}