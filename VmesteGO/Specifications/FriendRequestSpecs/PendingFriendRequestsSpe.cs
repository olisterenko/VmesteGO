using Ardalis.Specification;
using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;

namespace VmesteGO.Specifications.FriendRequestSpecs;

public sealed class PendingFriendRequestsSpec: Specification<FriendRequest>
{
    public PendingFriendRequestsSpec(int userId)
    {
        Query
            .Where(fr => fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending)
            .Include(fr => fr.Sender);
    }
}