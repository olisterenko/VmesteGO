using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.CommentSpecs;

public sealed class CommentWithUserRatingSpec : Specification<Comment>
{
    public CommentWithUserRatingSpec(int eventId)
    {
        Query
            .Include(r => r.Author)
            .Include(r => r.UserCommentRatings).ThenInclude(ur => ur.User)
            .Where(r => r.EventId == eventId)
            .OrderBy(r => r.CreatedAt);
    }
}