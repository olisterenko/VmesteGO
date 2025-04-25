using AutoMapper;
using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Requests;
using VmesteGO.Dto.Responses;
using VmesteGO.Extensions;
using VmesteGO.Services.Interfaces;
using VmesteGO.Specifications.UserSpecs;

namespace VmesteGO.Services;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;
    private readonly IS3StorageService _s3Service;

    public UserService(
        IJwtService jwtService,
        IRepository<User> userRepository,
        IMapper mapper,
        IS3StorageService s3Service)
    {
        _jwtService = jwtService;
        _userRepository = userRepository;
        _mapper = mapper;
        _s3Service = s3Service;
    }

    public async Task<string> RegisterUser(UserRegisterRequest userRegisterRequest)
    {
        var (passwordHash, salt) = PasswordHelper.HashPassword(userRegisterRequest.Password);

        var user = new User
        {
            Username = userRegisterRequest.Username,
            PasswordHash = passwordHash,
            Salt = salt,
            ImageKey = userRegisterRequest.ImageKey ?? $"users/default{new Random().Next(1, 6)}.jpg",
            Role = Role.User
        };

        await _userRepository.AddAsync(user);

        return _jwtService.GenerateToken(user.Id, user.Username, user.Role);
    }

    public async Task<string> LoginUser(UserLoginRequest userLoginRequest)
    {
        var userSpec = new UserByUsernameSpec(userLoginRequest.Username);
        var user = await _userRepository.FirstOrDefaultAsync(userSpec);

        if (user == null || !PasswordHelper.VerifyPassword(user.PasswordHash, user.Salt, userLoginRequest.Password))
        {
            throw new Exception("Invalid username or password.");
        }

        return _jwtService.GenerateToken(user.Id, user.Username, user.Role);
    }

    public async Task<UserResponse> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            ImageUrl = _s3Service.GetImageUrl(user.ImageKey),
            Role = user.Role
        };
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
    {
        var users = await _userRepository.ListAsync();

        return users.Select(u => u.ToUserResponse(_s3Service.GetImageUrl));
    }

    public async Task<UserResponse> UpdateUserAsync(int id, UserUpdateRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);

        _mapper.Map(request, user);

        if (!string.IsNullOrEmpty(request.Password))
        {
            var (salt, hash) = PasswordHelper.HashPassword(request.Password);
            user.Salt = salt;
            user.PasswordHash = hash;
        }

        await _userRepository.SaveChangesAsync();

        return user.ToUserResponse(_s3Service.GetImageUrl);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        _userRepository.Delete(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<UserResponse>> SearchUsersAsync(UserSearchRequest request,
        CancellationToken cancellationToken)
    {
        var spec = new UserSearchSpec(request.CurrentUserId, request.Username, request.Page, request.PageSize);
        var users = await _userRepository.ListAsync(spec, cancellationToken);

        return users.Select(u => new UserResponse
        {
            Id = u.Id,
            Username = u.Username,
            ImageUrl = _s3Service.GetImageUrl(u.ImageKey),
            Role = u.Role
        });
    }

    public async Task<UploadUserImageUrlResponse> GetUserUploadUrl(int id)
    {
        var key = $"users/{id}/profile.jpg";
        var url = await _s3Service.GenerateSignedUploadUrl(key);
        return new UploadUserImageUrlResponse(url, key);
    }

    public async Task<UserResponse> UpdateUserImageKey(int id, string key)
    {
        var user = await _userRepository.GetByIdAsync(id);
        user.ImageKey = key;

        await _userRepository.SaveChangesAsync();
        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            ImageUrl = _s3Service.GetImageUrl(user.ImageKey),
            Role = user.Role
        };
    }
}