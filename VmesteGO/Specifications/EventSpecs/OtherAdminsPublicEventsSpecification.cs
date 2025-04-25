using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.EventSpecs;

public sealed class OtherAdminsPublicEventsSpecification : Specification<Event>
{
    public OtherAdminsPublicEventsSpecification(int adminId, string? search, List<int>? categoryIds, int offset, int limit)
    {
        Query
            .Where(e => !e.IsPrivate && e.CreatorId != adminId)
            .Include(e => e.Creator);

        if (!string.IsNullOrWhiteSpace(search))
        {
            Query.Where(e =>
                e.Title.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                e.Description.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                e.Location.Contains(search, StringComparison.CurrentCultureIgnoreCase));
        }

        if (categoryIds is { Count: > 0 })
        {
            Query.Where(e => e.EventCategories
                .Any(ec => categoryIds.Contains(ec.CategoryId)));
        }

        Query
            .Include(e => e.EventImages)
            .Include(e => e.EventCategories)
            .ThenInclude(ec => ec.Category);

        Query.Skip(offset).Take(limit);
    }
}
