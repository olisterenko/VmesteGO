using VmesteGO.Domain.Enums;

namespace VmesteGO.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(string username, Role role);
}