using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.UserEventSpecs;

public sealed class UserEventByUserAndEventSpec : Specification<UserEvent>, ISingleResultSpecification<UserEvent>
{
    public UserEventByUserAndEventSpec(int userId, int eventId)
    {
        Query
            .Where(ue => ue.UserId == userId && ue.EventId == eventId);
    }
}