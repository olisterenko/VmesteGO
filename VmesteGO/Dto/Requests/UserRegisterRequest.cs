namespace VmesteGO.Dto.Requests;

public class UserRegisterRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? ImageUrl { get; set; } // TODO: base64
}