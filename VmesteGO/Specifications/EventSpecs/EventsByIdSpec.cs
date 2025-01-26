using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.EventSpecs;

public sealed class EventsByIdSpec : Specification<Event>, ISingleResultSpecification<Event>
{
    public EventsByIdSpec(int id)
    {
        Query
            .Where(e => e.Id == id)
            .Include(e => e.Creator)
            .Include(e => e.EventCategories).ThenInclude(ec => ec.Category)
            .Include(e => e.EventImages);
    }
}