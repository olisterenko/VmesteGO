using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.UserEventSpecs;

public sealed class FriendsEventsSpecification : Specification<UserEvent>
{
    public FriendsEventsSpecification(IEnumerable<int> friendIds)
    {
        Query.Where(ea => friendIds.Contains(ea.UserId))
            .Include(ea => ea.Event)
            .Include(ea => ea.User);
    }
}