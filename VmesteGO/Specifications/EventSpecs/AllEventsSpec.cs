using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.EventSpecs;

public sealed class AllEventsSpec : Specification<Event>
{
    public AllEventsSpec()
    {
        Query
            .Include(e => e.Creator)
            .Include(e => e.EventCategories).ThenInclude(ec => ec.Category)
            .Include(e => e.EventImages);
    }
}