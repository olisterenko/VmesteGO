using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.UserCommentRatingSpecs;

public sealed class UserCommentRatingSpec : Specification<UserCommentRating>, ISingleResultSpecification<UserCommentRating>
{
    public UserCommentRatingSpec(int userId, int commentId)
    {
        Query
            .Where(urc => urc.UserId == userId && urc.CommentId == commentId);
    }
}