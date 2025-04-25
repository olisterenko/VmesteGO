using AutoMapper;
using Moq;
using VmesteGO;
using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Requests;
using VmesteGO.Dto.Responses;
using VmesteGO.Exceptions;
using VmesteGO.Services;
using VmesteGO.Services.Interfaces;
using VmesteGO.Specifications.UserSpecs;

namespace VmesteGoTests;

[TestClass]
public class UserServiceTests
{
    private Mock<IRepository<User>> _userRepositoryMock;
    private Mock<IJwtService> _jwtServiceMock;
    private Mock<IMapper> _mapperMock;
    private Mock<IS3StorageService> _s3ServiceMock;
    private UserService _userService;

    [TestInitialize]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IRepository<User>>();
        _jwtServiceMock = new Mock<IJwtService>();
        _mapperMock = new Mock<IMapper>();
        _s3ServiceMock = new Mock<IS3StorageService>();

        _userService = new UserService(
            _jwtServiceMock.Object,
            _userRepositoryMock.Object,
            _mapperMock.Object,
            _s3ServiceMock.Object);
    }

    [TestMethod]
    public async Task RegisterUser_RegistersUserAndGeneratesToken()
    {
        // Arrange
        var registerRequest = new UserRegisterRequest
        {
            Username = "newUser",
            Password = "Password123!",
            ImageKey = "users/default1.jpg"
        };

        _userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken ct) =>
            {
                user.Id = 1;
                return user;
            });

        const string generatedToken = "token";
        _jwtServiceMock.Setup(jwt => jwt.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Role>()))
            .Returns(generatedToken);

        // Act
        var token = await _userService.RegisterUser(registerRequest);

        // Assert
        Assert.AreEqual(generatedToken, token);

        _userRepositoryMock.Verify(repo => repo.AddAsync(It.Is<User>(u =>
            u.Username == registerRequest.Username &&
            u.ImageKey == registerRequest.ImageKey &&
            u.Role == Role.User), It.IsAny<CancellationToken>()), Times.Once);

        _jwtServiceMock.Verify(jwt => jwt.GenerateToken(1, registerRequest.Username, Role.User), Times.Once);
    }

    [TestMethod]
    public async Task RegisterUser_RegistersUserWithDefaultImageKey_WhenImageKeyIsNull()
    {
        // Arrange
        var registerRequest = new UserRegisterRequest
        {
            Username = "newUser",
            Password = "Password123!"
        };

        _userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken ct) =>
            {
                user.Id = 2;
                return user;
            });

        const string generatedToken = "token";
        _jwtServiceMock.Setup(jwt => jwt.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Role>()))
            .Returns(generatedToken);

        // Act
        var token = await _userService.RegisterUser(registerRequest);

        // Assert
        Assert.AreEqual(generatedToken, token, "The generated token should match the mocked token.");

        _userRepositoryMock.Verify(repo => repo.AddAsync(It.Is<User>(u =>
            u.Username == registerRequest.Username &&
            u.ImageKey.StartsWith("users/default") &&
            u.ImageKey.EndsWith(".jpg") &&
            u.Role == Role.User), It.IsAny<CancellationToken>()), Times.Once);

        _jwtServiceMock.Verify(jwt => jwt.GenerateToken(2, registerRequest.Username, Role.User), Times.Once);
    }

    [TestMethod]
    public async Task LoginUser_LogsInAndGeneratesToken()
    {
        // Arrange
        var loginRequest = new UserLoginRequest
        {
            Username = "existingUser",
            Password = "Password123!"
        };

        var (passwordHash, salt) = PasswordHelper.HashPassword(loginRequest.Password);

        var user = new User
        {
            Id = 1,
            Username = "existingUser",
            PasswordHash = passwordHash,
            Salt = salt,
            Role = Role.User,
            ImageKey = "users/default1.jpg"
        };

        _userRepositoryMock.Setup(repo =>
                repo.FirstOrDefaultAsync(It.IsAny<UserByUsernameSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        const string generatedToken = "token";
        _jwtServiceMock.Setup(jwt => jwt.GenerateToken(user.Id, user.Username, user.Role))
            .Returns(generatedToken);

        // Act
        var token = await _userService.LoginUser(loginRequest);

        // Assert
        Assert.AreEqual(generatedToken, token, "The generated token should match the mocked token.");

        _userRepositoryMock.Verify(
            repo => repo.FirstOrDefaultAsync(It.IsAny<UserByUsernameSpec>(),
                It.IsAny<CancellationToken>()), Times.Once);
        _jwtServiceMock.Verify(jwt => jwt.GenerateToken(user.Id, user.Username, user.Role), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(EntityNotFoundException<User>))]
    public async Task LoginUser_ThrowsException_WhenUserDoesNotExist()
    {
        // Arrange
        var loginRequest = new UserLoginRequest
        {
            Username = "nonexistentUser",
            Password = "Password123!"
        };

        _userRepositoryMock.Setup(repo =>
                repo.FirstOrDefaultAsync(It.IsAny<UserByUsernameSpec>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException<User>(1));

        // Act
        await _userService.LoginUser(loginRequest);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task LoginUser_ThrowsException_WhenPasswordIsIncorrect()
    {
        // Arrange
        var loginRequest = new UserLoginRequest
        {
            Username = "existingUser",
            Password = "WrongPassword!"
        };

        var user = new User
        {
            Id = 1,
            Username = "existingUser",
            PasswordHash = "hashedPassword",
            Salt = "randomSalt",
            Role = Role.User,
            ImageKey = "users/default1.jpg"
        };

        _userRepositoryMock.Setup(repo =>
                repo.FirstOrDefaultAsync(It.IsAny<UserByUsernameSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtServiceMock.Setup(jwt => jwt.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Role>()))
            .Throws(new Exception("Invalid username or password."));

        // Act
        await _userService.LoginUser(loginRequest);
    }

    [TestMethod]
    public async Task GetUserByIdAsync_ReturnsUserResponse_WhenUserExists()
    {
        // Arrange
        const int userId = 1;
        var user = new User
        {
            Id = userId,
            Username = "existingUser",
            Role = Role.User,
            ImageKey = "users/default1.jpg"
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var userResponse = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role,
            ImageUrl = "https://storage.yandexcloud.net/bucket/users/default1.jpg"
        };

        _s3ServiceMock.Setup(s3 => s3.GetImageUrl(user.ImageKey))
            .Returns(userResponse.ImageUrl);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(userResponse.Id, result.Id);
        Assert.AreEqual(userResponse.Username, result.Username);
        Assert.AreEqual(userResponse.Role, result.Role);
        Assert.AreEqual(userResponse.ImageUrl, result.ImageUrl);

        _userRepositoryMock.Verify(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _s3ServiceMock.Verify(s3 => s3.GetImageUrl(user.ImageKey), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(EntityNotFoundException<User>))]
    public async Task GetUserByIdAsync_ThrowsException_WhenUserDoesNotExist()
    {
        // Arrange
        const int userId = 1;

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException<User>(1));

        // Act
        await _userService.GetUserByIdAsync(userId);
    }

    [TestMethod]
    public async Task GetAllUsersAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, Username = "user1", Role = Role.User, ImageKey = "users/user1.jpg" },
            new() { Id = 2, Username = "user2", Role = Role.Admin, ImageKey = "users/user2.jpg" }
        };

        _userRepositoryMock.Setup(repo => repo.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var userResponses = users.Select(u => new UserResponse
        {
            Id = u.Id,
            Username = u.Username,
            Role = u.Role,
            ImageUrl = $"https://storage.yandexcloud.net/bucket/{u.ImageKey}"
        });

        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<UserResponse>>(users))
            .Returns(userResponses);

        _s3ServiceMock.Setup(s3 => s3.GetImageUrl(It.IsAny<string>()))
            .Returns<string>(key => $"https://storage.yandexcloud.net/bucket/{key}");

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        Assert.IsNotNull(result);
        var enumerable = result.ToList();
        Assert.AreEqual(2, enumerable.Count);

        foreach (var user in users)
        {
            var response = enumerable.FirstOrDefault(r => r.Id == user.Id);
            Assert.IsNotNull(response);
            Assert.AreEqual(user.Username, response.Username);
            Assert.AreEqual(user.Role, response.Role);
            Assert.AreEqual($"https://storage.yandexcloud.net/bucket/{user.ImageKey}", response.ImageUrl);
        }

        _userRepositoryMock.Verify(repo => repo.ListAsync(It.IsAny<CancellationToken>()), Times.Once);
        _s3ServiceMock.Verify(s3 => s3.GetImageUrl(It.IsAny<string>()), Times.Exactly(users.Count));
    }

    [TestMethod]
    public async Task GetAllUsersAsync_ReturnsEmptyList_WhenNoUsersExist()
    {
        // Arrange
        var users = new List<User>();

        _userRepositoryMock.Setup(repo => repo.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());

        _userRepositoryMock.Verify(repo => repo.ListAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task UpdateUserAsync_UpdatesUser()
    {
        // Arrange
        const int userId = 1;
        var updateRequest = new UserUpdateRequest
        {
            Username = "updatedUser"
        };

        var user = new User
        {
            Id = userId,
            Username = "originalUser",
            Role = Role.User,
            ImageKey = "users/originalUser.jpg",
            PasswordHash = "originalHash",
            Salt = "originalSalt"
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mapperMock.Setup(mapper => mapper.Map(updateRequest, user))
            .Callback<UserUpdateRequest, User>((src, dest) =>
            {
                if (!string.IsNullOrEmpty(src.Username))
                    dest.Username = src.Username;
                if (!string.IsNullOrEmpty(src.ImageKey))
                    dest.ImageKey = src.ImageKey;
            });

        _s3ServiceMock.Setup(s3 => s3.GetImageUrl(user.ImageKey))
            .Returns($"https://storage.yandexcloud.net/bucket/{user.ImageKey}");

        _userRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _userService.UpdateUserAsync(userId, updateRequest);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(userId, result.Id);
        Assert.AreEqual(updateRequest.Username, result.Username);
        Assert.AreEqual(user.Role, result.Role);
        Assert.AreEqual($"https://storage.yandexcloud.net/bucket/{user.ImageKey}", result.ImageUrl);
        Assert.AreEqual("originalHash", user.PasswordHash, "PasswordHash should remain unchanged.");
        Assert.AreEqual("originalSalt", user.Salt, "Salt should remain unchanged.");

        _userRepositoryMock.Verify(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map(updateRequest, user), Times.Once);
        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _s3ServiceMock.Verify(s3 => s3.GetImageUrl(user.ImageKey), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(EntityNotFoundException<User>))]
    public async Task UpdateUserAsync_ThrowsException_WhenUserDoesNotExist()
    {
        // Arrange
        const int userId = 2;
        var updateRequest = new UserUpdateRequest
        {
            Username = "nonexistentuser",
            ImageKey = "users/nonexistentuser.jpg",
            Password = "Password123!"
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException<User>(2));

        // Act
        await _userService.UpdateUserAsync(userId, updateRequest);
    }

    [TestMethod]
    public async Task DeleteUserAsync_DeletesUser()
    {
        // Arrange
        const int userId = 3;
        var user = new User
        {
            Id = userId,
            Username = "deletableuser",
            Role = Role.User,
            ImageKey = "users/deletableuser.jpg"
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock.Setup(repo => repo.Delete(user));

        _userRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        Assert.IsTrue(result, "DeleteUserAsync should return true upon successful deletion.");
        _userRepositoryMock.Verify(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.Delete(user), Times.Once);
        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(EntityNotFoundException<User>))]
    public async Task DeleteUserAsync_ThrowsException_WhenUserDoesNotExist()
    {
        // Arrange
        const int userId = 2;

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException<User>(userId));

        // Act
        await _userService.DeleteUserAsync(userId);
    }

    [TestMethod]
    public async Task SearchUsersAsync_ReturnsMatchingUsers()
    {
        // Arrange
        var searchRequest = new UserSearchRequest
        {
            CurrentUserId = 1,
            Username = "searchuser",
            Page = 1,
            PageSize = 10
        };

        var users = new List<User>
        {
            new() { Id = 4, Username = "searchuser1", Role = Role.User, ImageKey = "users/searchuser1.jpg" },
            new() { Id = 5, Username = "searchuser2", Role = Role.Admin, ImageKey = "users/searchuser2.jpg" }
        };

        _userRepositoryMock.Setup(repo => repo.ListAsync(
                It.IsAny<UserSearchSpec>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var userResponses = users.Select(u => new UserResponse
        {
            Id = u.Id,
            Username = u.Username,
            Role = u.Role,
            ImageUrl = $"https://storage.yandexcloud.net/bucket/{u.ImageKey}"
        });

        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<UserResponse>>(users))
            .Returns(userResponses);

        _s3ServiceMock.Setup(s3 => s3.GetImageUrl(It.IsAny<string>()))
            .Returns<string>(key => $"https://storage.yandexcloud.net/bucket/{key}");

        // Act
        var result = await _userService.SearchUsersAsync(searchRequest, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        var responses = result.ToList();
        Assert.AreEqual(2, responses.Count);

        foreach (var user in users)
        {
            var response = responses.FirstOrDefault(r => r.Id == user.Id);
            Assert.IsNotNull(response);
            Assert.AreEqual(user.Username, response.Username);
            Assert.AreEqual(user.Role, response.Role);
            Assert.AreEqual($"https://storage.yandexcloud.net/bucket/{user.ImageKey}", response.ImageUrl);
        }

        _userRepositoryMock.Verify(repo => repo.ListAsync(
            It.IsAny<UserSearchSpec>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _s3ServiceMock.Verify(s3 => s3.GetImageUrl(It.IsAny<string>()), Times.Exactly(users.Count));
    }

    [TestMethod]
    public async Task SearchUsersAsync_ReturnsEmpty_WhenNoMatchingUsers()
    {
        // Arrange
        var searchRequest = new UserSearchRequest
        {
            CurrentUserId = 1,
            Username = "nomatchuser",
            Page = 1,
            PageSize = 10
        };

        var users = new List<User>();

        _userRepositoryMock.Setup(repo => repo.ListAsync(
                It.IsAny<UserSearchSpec>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);
        
        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<UserResponse>>(users))
            .Returns([]);

        // Act
        var result = await _userService.SearchUsersAsync(searchRequest, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());

        _userRepositoryMock.Verify(repo => repo.ListAsync(
            It.IsAny<UserSearchSpec>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task GetUserUploadUrl_ReturnsValidUploadUrl()
    {
        // Arrange
        const int userId = 6;
        var expectedKey = $"users/{userId}/profile.jpg";
        const string expectedUrl = "https://storage.yandexcloud.net/bucket/users/6/profile.jpg?signed=true";

        var user = new User
        {
            Id = userId,
            Username = "uploaduser",
            Role = Role.User,
            ImageKey = "users/6/profile.jpg"
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _s3ServiceMock.Setup(s3 => s3.GenerateSignedUploadUrl(expectedKey))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _userService.GetUserUploadUrl(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedUrl, result.UploadUrl);
        Assert.AreEqual(expectedKey, result.Key);

        _s3ServiceMock.Verify(s3 => s3.GenerateSignedUploadUrl(expectedKey), Times.Once);
    }

    [TestMethod]
    public async Task UpdateUserImageKey_SuccessfullyUpdatesImageKey()
    {
        // Arrange
        const int userId = 8;
        const string newKey = "users/8/newprofile.jpg";
        var user = new User
        {
            Id = userId,
            Username = "imageuser",
            Role = Role.User,
            ImageKey = "users/8/oldprofile.jpg"
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _s3ServiceMock.Setup(s3 => s3.GetImageUrl(newKey))
            .Returns($"https://storage.yandexcloud.net/bucket/{newKey}");

        // Act
        var result = await _userService.UpdateUserImageKey(userId, newKey);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(userId, result.Id);
        Assert.AreEqual("imageuser", result.Username);
        Assert.AreEqual(user.Role, result.Role);
        Assert.AreEqual($"https://storage.yandexcloud.net/bucket/{newKey}", result.ImageUrl);
        Assert.AreEqual(newKey, user.ImageKey);

        _userRepositoryMock.Verify(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _s3ServiceMock.Verify(s3 => s3.GetImageUrl(newKey), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(EntityNotFoundException<User>))]
    public async Task UpdateUserImageKey_ThrowsException_WhenUserDoesNotExist()
    {
        // Arrange
        const int userId = 9;
        const string newKey = "users/9/newprofile.jpg";

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException<User>("User not found."));

        // Act
        await _userService.UpdateUserImageKey(userId, newKey);
    }
}