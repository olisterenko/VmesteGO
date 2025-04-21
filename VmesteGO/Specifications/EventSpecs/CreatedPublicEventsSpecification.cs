using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.EventSpecs;

public sealed class CreatedPublicEventsSpecification : Specification<Event>
{
    public CreatedPublicEventsSpecification(int adminId, string? search, List<int>? categoryIds, int offset, int limit)
    {
        Query
            .Where(e => !e.IsPrivate && e.CreatorId == adminId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            Query.Where(e =>
                e.Title.Contains(search) ||
                e.Description.Contains(search) ||
                e.Location.Contains(search));
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