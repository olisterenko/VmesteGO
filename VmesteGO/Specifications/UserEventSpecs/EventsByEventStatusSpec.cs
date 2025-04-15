using Ardalis.Specification;
using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;

namespace VmesteGO.Specifications.UserEventSpecs;

public sealed class EventsByEventStatusSpec : Specification<UserEvent>
{
    public EventsByEventStatusSpec(int userId, EventStatus eventStatus)
    {
        Query
            .Where(ue => ue.UserId == userId && ue.EventStatus == eventStatus)
            .Include(ue => ue.Event);
    }
}