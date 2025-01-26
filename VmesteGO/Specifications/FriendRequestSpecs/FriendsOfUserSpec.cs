using Ardalis.Specification;
using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;

namespace VmesteGO.Specifications.FriendRequestSpecs;

public sealed class FriendsOfUserSpec : Specification<FriendRequest>
{
    public FriendsOfUserSpec(int userId)
    {
        Query
            .Where(f =>
                (f.SenderId == userId || f.ReceiverId == userId)
                && f.Status == FriendRequestStatus.Accepted)
            .Include(f => f.Receiver)
            .Include(f => f.Sender);
    }
}