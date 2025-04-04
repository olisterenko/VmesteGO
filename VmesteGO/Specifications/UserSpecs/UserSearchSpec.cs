using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.UserSpecs;

public sealed class UserSearchSpec : Specification<User>
{
    public UserSearchSpec(string? username, int page, int pageSize)
    {
        if (!string.IsNullOrWhiteSpace(username))
        {
            Query.Where(u => u.Username.Contains(username));
        }

        Query.Skip((page - 1) * pageSize).Take(pageSize);
    }
}

