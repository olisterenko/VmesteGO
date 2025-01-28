using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Requests;
using VmesteGO.Dto.Responses;
using VmesteGO.Services.Interfaces;
using VmesteGO.Specifications.UserSpecs;

namespace VmesteGO.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context; // TODO: get rid of it
    private readonly IRepository<User> _userRepository;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;

    public UserService(
        ApplicationDbContext context,
        IJwtService jwtService,
        IRepository<User> userRepository,
        IMapper mapper)
    {
        _context = context;
        _jwtService = jwtService;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<string> RegisterUser(UserRegisterRequest userRegisterRequest)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == userRegisterRequest.Username);

        if (existingUser != null)
        {
            throw new Exception("Username is already taken."); // TODO: вытащить в валидацию? 
        }

        // Hash the password and get the salt
        var (passwordHash, salt) = PasswordHelper.HashPassword(userRegisterRequest.Password);

        var user = new User // TODO: fix bug with hashing (creates user if failed)
        {
            Username = userRegisterRequest.Username,
            PasswordHash = passwordHash,
            Salt = salt,
            ImageUrl = userRegisterRequest.ImageUrl ?? "someUrl", // TODO: заменить на норм урл
            Role = Role.User
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return _jwtService.GenerateToken(user.Id, user.Username, user.Role);
    }

    public async Task<string> LoginUser(UserLoginRequest userLoginRequest)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == userLoginRequest.Username); // TODO: переделать под спецификации?

        if (user == null || !PasswordHelper.VerifyPassword(user.PasswordHash, user.Salt, userLoginRequest.Password))
        {
            throw new Exception("Invalid username or password.");
        }

        return _jwtService.GenerateToken(user.Id, user.Username, user.Role);
    }

    public async Task<UserResponse> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
    {
        var users = await _userRepository.ListAsync();
        return _mapper.Map<IEnumerable<UserResponse>>(users);
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

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        _userRepository.Delete(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }
}