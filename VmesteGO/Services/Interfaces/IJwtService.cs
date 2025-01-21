using VmesteGO.Domain.Enums;

namespace VmesteGO.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(int userId, string username, Role role);
}