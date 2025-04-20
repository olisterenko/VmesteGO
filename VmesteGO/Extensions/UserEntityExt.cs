using VmesteGO.Domain.Entities;
using VmesteGO.Dto.Responses;

namespace VmesteGO.Extensions;

public static class UserEntityExt
{
    public static UserResponse ToUserResponse(this User user, Func<string, string> getImageUrlByKey)
    {
        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            ImageUrl = getImageUrlByKey(user.ImageKey),
            Role = user.Role
        };
    }
}