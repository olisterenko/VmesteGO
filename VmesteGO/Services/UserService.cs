using Microsoft.EntityFrameworkCore;
using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Requests;
using VmesteGO.Services.Interfaces;

namespace VmesteGO.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public UserService(ApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
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

        var user = new User
        {
            Username = userRegisterRequest.Username,
            PasswordHash = passwordHash,
            Salt = salt,
            ImageUrl = userRegisterRequest.ImageUrl ?? "someUrl", // TODO: заменить на норм урл
            Role = Role.User // TODO: enum?
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return _jwtService.GenerateToken(user.Username, user.Role);
    }

    public async Task<string> LoginUser(UserLoginRequest userLoginRequest)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == userLoginRequest.Username); // TODO: переделать под спецификации?
        
        if (user == null || !PasswordHelper.VerifyPassword(user.PasswordHash, user.Salt, userLoginRequest.Password))
        {
            throw new Exception("Invalid username or password.");
        }

        return _jwtService.GenerateToken(user.Username, user.Role);
    }
}