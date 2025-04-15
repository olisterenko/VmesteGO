using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VmesteGO.Dto.Requests;
using VmesteGO.Dto.Responses;
using VmesteGO.Services.Interfaces;

namespace VmesteGO.Controllers;

    [ApiController]
    [Route("comments")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IUserContext _userContext;

        public CommentsController(ICommentService commentService, IUserContext userContext)
        {
            _commentService = commentService;
            _userContext = userContext;
        }
        
        [HttpGet("{eventId:int}")] // TODO: подумать тут с авторизацией (как и везде блин)
        public async Task<List<GetCommentResponse>> GetComments(int eventId)
        {
            var userId = _userContext.UserId;

            return await _commentService.GetComments(
                new GetCommentRequest
                {
                    UserId = userId,
                    EventId = eventId
                }
            );
        }

        [HttpPost]
        public async Task PostComment(PostCommentRequest postCommentRequest)
        {
            var userId = _userContext.UserId;

            await _commentService.PostComment(userId, postCommentRequest);
        }

        [HttpPost("{commentId:int}")]
        public async Task RateComment(int commentId, [FromBody] bool isPositive)
        {
            var userId = _userContext.UserId;

            await _commentService.RateComment(userId, commentId, isPositive);
        }
        
        [HttpDelete("{commentId:int}")]
        public async Task DeleteComment(int commentId, CancellationToken cancellationToken = default)
        {
            var userId = _userContext.UserId;
            
            await _commentService.DeleteCommentAsync(userId, commentId, cancellationToken);
        }
    }