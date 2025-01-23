using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.FriendRequestSpecs;

public sealed class SentFriendRequestsSpec: Specification<FriendRequest>
{
    public SentFriendRequestsSpec(int userId)
    {
        Query
            .Where(fr => fr.SenderId == userId)
            .Include(fr => fr.Receiver)
            .Include(fr => fr.Sender);
    }
}
