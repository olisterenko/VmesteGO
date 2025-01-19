using VmesteGO.Dto.Requests;

namespace VmesteGO.Services.Interfaces;

public interface IUserService
{
    Task<string> RegisterUser(UserRegisterRequest userRegisterRequest);
    Task<string> LoginUser(UserLoginRequest userLoginRequest);
}