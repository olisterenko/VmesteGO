using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.EventInvitationSpecs;

public sealed class SentEventInvitationsSpec : Specification<EventInvitation>
{
    public SentEventInvitationsSpec(int senderId)
    {
        Query.Where(ei => ei.SenderId == senderId)
            .Include(ei => ei.Event)
            .Include(ei => ei.Sender)
            .Include(ei => ei.Receiver);
    }
}