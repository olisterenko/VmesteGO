using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.EventSpecs;

public sealed class CreatedPublicEventsSpecification : Specification<Event>
{
    public CreatedPublicEventsSpecification(int adminId, string search, int offset, int limit)
    {
        Query.Where(e => !e.IsPrivate && e.CreatorId == adminId && e.Title.Contains(search))
            .Skip(offset).Take(limit);
    }
}