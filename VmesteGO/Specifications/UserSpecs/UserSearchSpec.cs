using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.UserSpecs;

public sealed class UserSearchSpec : Specification<User>
{
    public UserSearchSpec(int currentUserId, string? username, int page, int pageSize)
    {
        Query.Where(u => u.Id != currentUserId);
        
        if (!string.IsNullOrWhiteSpace(username))
        {
            Query.Where(u => u.Username.Contains(username));
        }

        Query.Skip((page - 1) * pageSize).Take(pageSize);
    }
}

