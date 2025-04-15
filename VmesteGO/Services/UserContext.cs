using System.Security.Claims;
using VmesteGO.Services.Interfaces;

namespace VmesteGO.Services;

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !int.TryParse(claim.Value, out var userId))
                throw new UnauthorizedAccessException("User ID claim not found.");

            return userId;
        }
    }
    
    public int? UserIdUnsafe
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !int.TryParse(claim.Value, out var userId))
                return null;

            return userId;
        }
    }

    public string Username
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name);
            return claim?.Value ?? throw new UnauthorizedAccessException("Username claim not found.");
        }
    }

    public string Role
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role);
            return claim?.Value ?? throw new UnauthorizedAccessException("Role claim not found.");
        }
    }
}