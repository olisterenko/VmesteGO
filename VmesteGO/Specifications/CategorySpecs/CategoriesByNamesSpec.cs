using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.CategorySpecs;

public sealed class CategoriesByNamesSpec : Specification<Category>
{
    public CategoriesByNamesSpec(IEnumerable<string> names)
    {
        Query
            .Where(c => names.Contains(c.Name.ToLower()));
    }
}