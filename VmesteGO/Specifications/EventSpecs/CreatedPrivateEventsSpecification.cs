using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.EventSpecs;

public sealed class CreatedPrivateEventsSpecification : Specification<Event>
{
    public CreatedPrivateEventsSpecification(int userId, string search, int offset, int limit)
    {
        Query.Where(e => e.IsPrivate && e.CreatorId == userId && e.Title.Contains(search))
            .Skip(offset).Take(limit);
    }
}