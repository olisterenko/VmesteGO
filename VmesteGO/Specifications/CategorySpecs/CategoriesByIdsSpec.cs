using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.CategorySpecs;

public sealed class CategoriesByIdsSpec : Specification<Category>
{
    public CategoriesByIdsSpec(IEnumerable<int> ids)
    {
        Query
            .Where(c => ids.Contains(c.Id));
    }
}