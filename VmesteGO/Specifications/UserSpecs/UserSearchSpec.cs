using Ardalis.Specification;
using VmesteGO.Domain.Entities;
using VmesteGO.Dto.Responses;

namespace VmesteGO.Specifications.UserSpecs;

public sealed class UserSearchSpec : Specification<User, UserResponse>
{
    public UserSearchSpec(string? username, int page, int pageSize)
    {
        if (!string.IsNullOrWhiteSpace(username))
        {
            Query.Where(u => u.Username.Contains(username));
        }

        Query.Skip((page - 1) * pageSize).Take(pageSize);

        Query.Select(u => new UserResponse
        {
            Id = u.Id,
            Username = u.Username,
            Role = u.Role,
            ImageUrl = u.ImageUrl
        });
    }
}

