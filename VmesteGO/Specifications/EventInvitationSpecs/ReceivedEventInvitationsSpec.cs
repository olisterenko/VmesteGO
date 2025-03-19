using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.EventInvitationSpecs;

public sealed class ReceivedEventInvitationsSpec : Specification<EventInvitation>
{
    public ReceivedEventInvitationsSpec(int receiverId)
    {
        Query.Where(ei => ei.ReceiverId == receiverId)
            .Include(ei => ei.Event)
            .Include(ei => ei.Sender)
            .Include(ei => ei.Receiver);
    }
}