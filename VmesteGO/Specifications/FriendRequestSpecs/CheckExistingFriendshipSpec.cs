using Ardalis.Specification;
using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;

namespace VmesteGO.Specifications.FriendRequestSpecs;

public sealed class CheckExistingFriendshipSpec : Specification<FriendRequest>, ISingleResultSpecification<FriendRequest>
{
    public CheckExistingFriendshipSpec(int userId, int friendUserId)
    {
        Query
            .Where(f =>
                ((f.SenderId == userId && f.ReceiverId == friendUserId) ||
                 (f.SenderId == friendUserId && f.ReceiverId == userId))
                && f.Status == FriendRequestStatus.Accepted);
    }
}