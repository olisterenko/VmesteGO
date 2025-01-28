using VmesteGO.Domain.Enums;

namespace VmesteGO.Dto.Responses;

public class UserResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public Role Role { get; set; }
    public string ImageUrl { get; set; } = null!;
}