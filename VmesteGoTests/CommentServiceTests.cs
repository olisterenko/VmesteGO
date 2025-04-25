using Moq;
using VmesteGO;
using VmesteGO.Domain.Entities;
using VmesteGO.Dto.Requests;
using VmesteGO.Services;
using VmesteGO.Specifications.CommentSpecs;
using VmesteGO.Specifications.UserCommentRatingSpecs;

namespace VmesteGoTests;

[TestClass]
public class CommentServiceTests
{
    private Mock<IRepository<Comment>> _commentRepoMock;
    private Mock<IRepository<UserCommentRating>> _ratingRepoMock;
    private CommentService _service;

    [TestInitialize]
    public void Setup()
    {
        _commentRepoMock = new Mock<IRepository<Comment>>();
        _ratingRepoMock = new Mock<IRepository<UserCommentRating>>();
        _service = new CommentService(_commentRepoMock.Object, _ratingRepoMock.Object);
    }

    [TestMethod]
    public async Task PostComment_AddsNewComment()
    {
        // Arrange
        var request = new PostCommentRequest { EventId = 1, Text = "Test Comment" };
        const int userId = 1;

        // Act
        await _service.PostComment(userId, request);

        // Assert
        _commentRepoMock.Verify(repo =>
                repo.AddAsync(
                    It.Is<Comment>(c =>
                        c.AuthorId == userId &&
                        c.Text == request.Text &&
                        c.EventId == request.EventId &&
                        c.Rating == 0),
                    CancellationToken.None),
            Times.Once);
    }

    [TestMethod]
    public async Task RateComment_AddsNewRating_WhenNoneExists()
    {
        // Arrange
        const int userId = 1;
        const int commentId = 42;

        _ratingRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<UserCommentRatingSpec>(), CancellationToken.None))
            .ReturnsAsync((UserCommentRating)null);

        // Act
        await _service.RateComment(userId, commentId, true);

        // Assert
        _ratingRepoMock.Verify(r =>
                r.AddAsync(It.Is<UserCommentRating>(u =>
                        u.UserId == userId &&
                        u.CommentId == commentId &&
                        u.UserRating == 1),
                    CancellationToken.None),
            Times.Once);

        _ratingRepoMock.Verify(r => r.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [TestMethod]
    public async Task RateComment_UpdatesRating_WhenExists()
    {
        // Arrange
        const int userId = 1;
        const int commentId = 42;
        var existing = new UserCommentRating { UserId = userId, CommentId = commentId, UserRating = -1 };

        _ratingRepoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<UserCommentRatingSpec>(), CancellationToken.None))
            .ReturnsAsync(existing);

        // Act
        await _service.RateComment(userId, commentId, true);

        // Assert
        Assert.AreEqual(1, existing.UserRating);
        _ratingRepoMock.Verify(r => r.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [TestMethod]
    public async Task DeleteCommentAsync_Deletes_WhenAuthorMatches()
    {
        // Arrange
        const int userId = 1;
        var comment = new Comment
        {
            Id = 3,
            AuthorId = userId,
            Text = "some text"
        };

        _commentRepoMock.Setup(r => r.GetByIdAsync(comment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        // Act
        await _service.DeleteCommentAsync(userId, comment.Id);

        // Assert
        _commentRepoMock.Verify(r => r.DeleteAsync(comment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAccessException))]
    public async Task DeleteCommentAsync_Throws_WhenAuthorMismatch()
    {
        // Arrange
        var comment = new Comment
        {
            Id = 2,
            AuthorId = 42,
            Text = "some text"
        };

        _commentRepoMock.Setup(r => r.GetByIdAsync(comment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        // Act
        await _service.DeleteCommentAsync(userId: 1, commentId: comment.Id);
    }

    [TestMethod]
    public async Task GetComments_ReturnsExpectedResponses()
    {
        // Arrange
        var request = new GetCommentRequest { EventId = 10, UserId = 1 };
        var comments = new List<Comment>
        {
            new()
            {
                Id = 1,
                EventId = 10,
                Text = "Comment 1",
                AuthorId = 2,
                CreatedAt = DateTime.UtcNow,
                Author = new User { Id = 2, Username = "Alice" },
                UserCommentRatings =
                [
                    new UserCommentRating { UserId = 1, UserRating = 1 },
                    new UserCommentRating { UserId = 3, UserRating = -1 }
                ]
            },
            new()
            {
                Id = 2,
                EventId = 10,
                Text = "Comment 2",
                AuthorId = 3,
                CreatedAt = DateTime.UtcNow,
                Author = new User { Id = 3, Username = "Bob" },
                UserCommentRatings = []
            }
        };

        _commentRepoMock.Setup(r => r.ListAsync(It.IsAny<CommentWithUserRatingSpec>(), CancellationToken.None))
            .ReturnsAsync(comments);

        // Act
        var result = await _service.GetComments(request);

        // Assert
        Assert.AreEqual(2, result.Count);

        var first = result.First();
        Assert.AreEqual(1, first.Id);
        Assert.AreEqual("Alice", first.AuthorUsername);
        Assert.AreEqual(0, result.Last().Rating);
        Assert.AreEqual(0, result.Last().UserRating);
    }
}