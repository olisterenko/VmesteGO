namespace VmesteGO.Services.Interfaces;

public interface IUserContext
{
    int UserId { get; }
    int? UserIdUnsafe { get; }
    string Username { get; }
    string Role { get; }
}