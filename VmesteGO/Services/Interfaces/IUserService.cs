using VmesteGO.Domain.Entities;
using VmesteGO.Dto.Requests;
using VmesteGO.Dto.Responses;

namespace VmesteGO.Services.Interfaces;

public interface IUserService
{
    Task<string> RegisterUser(UserRegisterRequest userRegisterRequest);
    Task<string> LoginUser(UserLoginRequest userLoginRequest);
    Task<UserResponse> GetUserByIdAsync(int id);
    Task<IEnumerable<UserResponse>> GetAllUsersAsync();
    Task<UserResponse> UpdateUserAsync(int id, UserUpdateRequest request);
    Task<bool> DeleteUserAsync(int id);
}