using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.FriendSpecs;

public sealed class FriendsOfUserSpec : Specification<Friend>
{
    public FriendsOfUserSpec(int userId)
    {
        Query
            .Where(f => f.UserId == userId || f.FriendUserId == userId)
            .Include(f => f.FriendUser)
            .Include(f => f.User);
    }
}