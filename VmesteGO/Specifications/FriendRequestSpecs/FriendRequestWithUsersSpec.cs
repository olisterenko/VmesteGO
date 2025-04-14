using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.FriendRequestSpecs;

public sealed class FriendRequestWithUsersSpec : Specification<FriendRequest>, ISingleResultSpecification<FriendRequest>
{
    public FriendRequestWithUsersSpec(int requestId)
    {
        Query
            .Where(f => f.Id == requestId)
            .Include(f => f.Receiver)
            .Include(f => f.Sender);
    }
}