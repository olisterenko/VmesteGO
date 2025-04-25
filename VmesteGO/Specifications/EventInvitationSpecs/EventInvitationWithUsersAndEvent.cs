using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.EventInvitationSpecs;

public sealed class EventInvitationWithUsersAndEvent : Specification<EventInvitation>, ISingleResultSpecification<EventInvitation>
{
    public EventInvitationWithUsersAndEvent(int inviteId)
    {
        Query.Where(ei => ei.Id == inviteId)
            .Include(ei => ei.Sender)
            .Include(ei => ei.Receiver)
            .Include(ei => ei.Event);
    }
}