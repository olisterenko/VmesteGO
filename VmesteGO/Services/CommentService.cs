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
    private readonly IRepository<UserCommentRating> _userRatingCommentRepository;

    public CommentService(
        IRepository<Comment> commentRepository,
        IRepository<UserCommentRating> userRatingCommentRepository)
    {
        _commentRepository = commentRepository;
        _userRatingCommentRepository = userRatingCommentRepository;
    }

    public async Task<List<GetCommentResponse>> GetComments(GetCommentRequest commentRequest)
    {
        var comments = await _commentRepository
            .ListAsync(new CommentWithUserRatingSpec(commentRequest.ChapterId));

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
            CreatedAt = DateTime.Now,
        };

        await _commentRepository.AddAsync(comment);
    }

    public async Task RateComment(int userId, int commentId, bool isPositive)
    {
        var userRatingComment =
            await _userRatingCommentRepository.FirstOrDefaultAsync(new UserRatingCommentSpec(userId, commentId));

        if (userRatingComment is null)
        {
            userRatingComment = new UserCommentRating
            {
                CommentId = commentId,
                UserId = userId,
                UserRating = isPositive ? 1 : -1
            };

            await _userRatingCommentRepository.AddAsync(userRatingComment);
        }

        userRatingComment.UserRating = isPositive ? 1 : -1;

        await _userRatingCommentRepository.SaveChangesAsync();
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