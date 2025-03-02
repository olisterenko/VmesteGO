namespace VmesteGO.Dto.Responses;

public record GetCommentResponse(
    int Id,
    int AuthorId,
    string AuthorUsername,
    string Text,
    int Rating,
    int UserRating,
    DateTimeOffset CreatedAt
);