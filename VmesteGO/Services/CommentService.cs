using VmesteGO.Domain.Entities;
using VmesteGO.Dto.Requests;
using VmesteGO.Dto.Responses;
using VmesteGO.Services.Interfaces;
using VmesteGO.Specifications.CommentSpecs;
using VmesteGO.Specifications.UserCommentRatingSpecs;

namespace VmesteGO.Services;

public class CommentService : ICommentService
{
    private readonly IRepository<Comment> _commentRepository;
    private readonly IRepository<UserCommentRating> _userCommentRatingRepository;

    public CommentService(
        IRepository<Comment> commentRepository,
        IRepository<UserCommentRating> userCommentRatingRepository)
    {
        _commentRepository = commentRepository;
        _userCommentRatingRepository = userCommentRatingRepository;
    }

    public async Task<List<GetCommentResponse>> GetComments(GetCommentRequest commentRequest)
    {
        var comments = await _commentRepository
            .ListAsync(new CommentWithUserRatingSpec(commentRequest.EventId));

        return comments
            .Select(comment => GetCommentResponse(commentRequest, comment))
            .ToList();
    }

    public async Task PostComment(int userId, PostCommentRequest postCommentRequest)
    {
        var comment = new Comment
        {
            EventId = postCommentRequest.EventId,
            Text = postCommentRequest.Text,
            AuthorId = userId,
            Rating = 0,
            CreatedAt = DateTime.UtcNow,
        };

        await _commentRepository.AddAsync(comment);
    }

    public async Task RateComment(int userId, int commentId, bool isPositive)
    {
        var userCommentRating =
            await _userCommentRatingRepository.FirstOrDefaultAsync(new UserCommentRatingSpec(userId, commentId));

        if (userCommentRating is null)
        {
            userCommentRating = new UserCommentRating
            {
                CommentId = commentId,
                UserId = userId,
                UserRating = isPositive ? 1 : -1
            };

            await _userCommentRatingRepository.AddAsync(userCommentRating);
        }

        userCommentRating.UserRating = isPositive ? 1 : -1;

        await _userCommentRatingRepository.SaveChangesAsync();
    }

    public async Task DeleteCommentAsync(int userId, int commentId, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId, cancellationToken);

        if (comment.AuthorId != userId)
            throw new UnauthorizedAccessException("You can only delete your own comments.");

        _commentRepository.Delete(comment);
    }

    private GetCommentResponse GetCommentResponse(GetCommentRequest commentRequest, Comment comment)
    {
        var userRating = comment.UserCommentRatings
            .FirstOrDefault(ur => ur.UserId == commentRequest.UserId)?
            .UserRating ?? 0;


        return new GetCommentResponse(
            Id: comment.Id,
            AuthorId: comment.AuthorId,
            AuthorUsername: comment.Author.Username,
            Text: comment.Text,
            Rating: comment.UserCommentRatings.Sum(ur => ur.UserRating),
            UserRating: userRating,
            CreatedAt: comment.CreatedAt
        );
    }
}