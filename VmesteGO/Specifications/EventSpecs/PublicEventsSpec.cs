using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.EventSpecs;

public sealed class PublicEventsSpec : Specification<Event>
{
    public PublicEventsSpec()
    {
        Query
            .Where(e => !e.IsPrivate)
            .Include(e => e.Creator)
            .Include(e => e.EventCategories).ThenInclude(ec => ec.Category)
            .Include(e => e.EventImages);
    }
}