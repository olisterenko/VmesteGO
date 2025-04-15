using VmesteGO.Dto.Requests;
using VmesteGO.Dto.Responses;

namespace VmesteGO.Services.Interfaces;

public interface ICommentService
{
    Task<List<GetCommentResponse>> GetComments(GetCommentRequest commentRequest);
    Task PostComment(int userId, PostCommentRequest postCommentRequest);
    Task RateComment(int userId, int commentId, bool isPositive);
    Task DeleteCommentAsync(int userId, int commentId, CancellationToken cancellationToken = default);
}