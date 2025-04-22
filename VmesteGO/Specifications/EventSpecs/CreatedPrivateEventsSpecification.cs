using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.EventSpecs;

public sealed class CreatedPrivateEventsSpecification : Specification<Event>
{
    public CreatedPrivateEventsSpecification(int userId, string? search, List<int>? categoryIds, int offset, int limit)
    {
        Query
            .Where(e => e.IsPrivate && e.CreatorId == userId)
            .Include(e => e.Creator);

        if (!string.IsNullOrWhiteSpace(search))
        {
            Query.Where(e =>
                e.Title.ToLower().Contains(search) ||
                e.Description.ToLower().Contains(search) ||
                e.Location.ToLower().Contains(search));
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