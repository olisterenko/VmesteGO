using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.UserCommentRatingSpecs;

public sealed class UserRatingCommentSpec : Specification<UserCommentRating>, ISingleResultSpecification<UserCommentRating>
{
    public UserRatingCommentSpec(int userId, int commentId)
    {
        Query
            .Where(urc => urc.UserId == userId && urc.CommentId == commentId);
    }
}