using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.FriendRequestSpecs;

public sealed class GetFriendRequestForUsersSpec: SingleResultSpecification<FriendRequest>
{
    public GetFriendRequestForUsersSpec(int fromId, int toId)
    {
        Query
            .Where(fr => fr.ReceiverId == toId)
            .Where(fr => fr.SenderId == fromId)
            .Include(fr => fr.Sender)
            .Include(fr => fr.Receiver);
    }
}