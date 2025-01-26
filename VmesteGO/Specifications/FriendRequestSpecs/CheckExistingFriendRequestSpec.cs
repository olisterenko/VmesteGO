using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.FriendRequestSpecs;

public sealed class CheckExistingFriendRequestSpec : Specification<FriendRequest> //TODO: ISingle here and check others
{
    public CheckExistingFriendRequestSpec(int senderId, int receiverId)
    {
        Query
            .Where(f =>
                (f.SenderId == senderId && f.ReceiverId == receiverId) ||
                (f.SenderId == receiverId && f.ReceiverId == senderId));
    }
}