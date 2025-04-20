namespace VmesteGO.Dto.Requests;

public class UserSearchRequest
{
    public int CurrentUserId { get; set; }
    public string? Username { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}