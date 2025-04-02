using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.EventSpecs;

public sealed class JoinedPrivateEventsSpecification : Specification<Event>
{
    public JoinedPrivateEventsSpecification(int userId, string search, int offset, int limit)
    {
        Query.Where(e => e.IsPrivate && e.EventInvitations.Any(ui => ui.ReceiverId == userId) && e.Title.Contains(search))
            .Skip(offset).Take(limit);
    }
}