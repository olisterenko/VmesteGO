using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.FriendSpecs;

public sealed class CheckExistingFriendshipSpec : Specification<Friend>
{
    public CheckExistingFriendshipSpec(int userId, int friendUserId)
    {
        Query
            .Where(f => 
            (f.UserId == userId && f.FriendUserId == friendUserId) || 
            (f.UserId == friendUserId && f.FriendUserId == userId));
    }
}