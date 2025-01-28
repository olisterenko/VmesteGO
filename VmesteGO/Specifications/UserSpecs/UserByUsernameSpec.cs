using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.UserSpecs;

public sealed class UserByUsernameSpec : Specification<User>, ISingleResultSpecification<User>
{
    public UserByUsernameSpec(string username)
    {
        Query.Where(u => u.Username == username);
    }
}