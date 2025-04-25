using Ardalis.Specification;
using AutoMapper;
using Moq;
using VmesteGO;
using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Requests;
using VmesteGO.Services;
using VmesteGO.Services.Interfaces;

namespace VmesteGoTests;

[TestClass]
public class EventServiceTests
{
    private Mock<IRepository<Event>> _eventRepoMock;
    private Mock<IRepository<User>> _userRepoMock;
    private Mock<IRepository<Category>> _categoryRepoMock;
    private Mock<IRepository<UserEvent>> _userEventRepoMock;
    private Mock<IMapper> _mapperMock;
    private Mock<IS3StorageService> _s3Mock;
    private Mock<IRepository<EventImage>> _eventImageRepoMock;
    private EventService _eventService;

    [TestInitialize]
    public void Setup()
    {
        _eventRepoMock = new Mock<IRepository<Event>>();
        _userRepoMock = new Mock<IRepository<User>>();
        _categoryRepoMock = new Mock<IRepository<Category>>();
        _userEventRepoMock = new Mock<IRepository<UserEvent>>();
        _mapperMock = new Mock<IMapper>();
        _s3Mock = new Mock<IS3StorageService>();
        _eventImageRepoMock = new Mock<IRepository<EventImage>>();

        _eventService = new EventService(
            _eventRepoMock.Object,
            _userRepoMock.Object,
            _categoryRepoMock.Object,
            _userEventRepoMock.Object,
            _mapperMock.Object,
            _s3Mock.Object,
            _eventImageRepoMock.Object
        );
    }

    [TestMethod]
    public async Task GetEventByIdAsync_ReturnsMappedResponse()
    {
        // Arrange
        var testEvent = new Event
        {
            Id = 1,
            Title = "Икар",
            Location = "КЗ Измайлово",
            Description = "Мюзикл"
        };
        _eventRepoMock.Setup(r => r.FirstAsync(It.IsAny<ISpecification<Event>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testEvent);

        _s3Mock.Setup(s => s.GetImageUrl(It.IsAny<string>())).Returns("http://image");

        // Act
        var result = await _eventService.GetEventByIdAsync(1);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
    }

    [TestMethod]
    public async Task GetEventByIdForUserAsync_ReturnsEvent()
    {
        // Arrange
        var testEvent = new Event
        {
            Id = 5,
            Title = "Икар",
            Location = "КЗ Измайлово",
            Description = "Мюзикл"
        };
        var testUserEvent = new UserEvent { EventId = 5, UserId = 2, EventStatus = EventStatus.Going };

        _eventRepoMock.Setup(x => x.FirstAsync(It.IsAny<ISpecification<Event>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testEvent);
        _userEventRepoMock.Setup(x =>
                x.FirstOrDefaultAsync(It.IsAny<ISpecification<UserEvent>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUserEvent);

        // Act
        var result = await _eventService.GetEventByIdForUserAsync(2, 5);

        // Assert
        Assert.AreEqual(5, result.Id);
        Assert.AreEqual(EventStatus.Going, result.EventStatus);
    }

    [TestMethod]
    public async Task CreateEventAsync_CreatesEvent()
    {
        // Arrange
        var request = new CreateEventRequest
        {
            Title = "Икар",
            EventCategoryNames = new List<string> { "rock" },
            EventImages = new List<string> { "key1" },
            Location = "КЗ Измайлово",
            Description = "Мюзикл"
        };

        var user = new User { Id = 10 };

        var eventEntity = new Event
        {
            Title = "Икар",
            Location = "КЗ Измайлово",
            Description = "Мюзикл"
        };

        _userRepoMock
            .Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mapperMock
            .Setup(x => x.Map<Event>(It.IsAny<CreateEventRequest>()))
            .Returns(eventEntity);

        _categoryRepoMock
            .Setup(x => x.ListAsync(It.IsAny<ISpecification<Category>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _categoryRepoMock
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<Category>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Category> categories, CancellationToken _) => categories);

        _eventRepoMock.Setup(x => x.Add(It.IsAny<Event>()));

        _eventRepoMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _userEventRepoMock
            .Setup(x => x.AddAsync(It.IsAny<UserEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEvent userEvent, CancellationToken _) => userEvent);

        // Act
        var result = await _eventService.CreateEventAsync(request, 10, Role.User);

        // Assert
        Assert.IsNotNull(result);
    }


    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAccessException))]
    public async Task UpdateEventAsync_WithUnauthorizedUser_ThrowsException()
    {
        // Arrange
        var eventToUpdate = new Event
        {
            Id = 1,
            CreatorId = 42,
            Title = "Икар",
            Location = "КЗ Измайлово",
            Description = "Мюзикл"
        };
        _eventRepoMock.Setup(x => x.FirstAsync(It.IsAny<ISpecification<Event>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventToUpdate);

        // Act
        await _eventService.UpdateEventAsync(1, new UpdateEventRequest
        {
            Title = "Икар",
            Location = "КЗ Измайлово",
            Description = "Мюзикл"
        }, userId: 2, role: Role.User);
    }

    [TestMethod]
    public async Task DeleteEventAsync_WithValidUser_DeletesEvent()
    {
        // Arrange
        var evt = new Event
        {
            Id = 2,
            CreatorId = 3,
            Title = "Икар",
            Location = "КЗ Измайлово",
            Description = "Мюзикл"
        };

        _eventRepoMock
            .Setup(x => x.GetByIdAsync(2, CancellationToken.None))
            .ReturnsAsync(evt);

        _eventRepoMock
            .Setup(x => x.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(1);

        // Act
        await _eventService.DeleteEventAsync(2, 3, Role.User);

        // Assert
        _eventRepoMock.Verify(x => x.Delete(evt), Times.Once);
        _eventRepoMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }


    [TestMethod]
    public async Task ChangeEventStatus_CreatesNewUserEvent()
    {
        // Arrange
        var request = new ChangeEventStatusRequest
        {
            EventId = 1,
            UserId = 2,
            NewEventStatus = EventStatus.Going
        };

        _userEventRepoMock
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<ISpecification<UserEvent>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEvent)null);

        _userEventRepoMock
            .Setup(x => x.AddAsync(It.IsAny<UserEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEvent userEvent, CancellationToken _) => userEvent);

        // Act
        await _eventService.ChangeEventStatus(request);

        // Assert
        _userEventRepoMock.Verify(x => x.AddAsync(It.IsAny<UserEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }


    [TestMethod]
    public async Task GetEventUploadUrl_ReturnsSignedUrl()
    {
        // Arrange
        var evt = new Event
        {
            Id = 3,
            CreatorId = 1,
            Title = "Икар",
            Location = "КЗ Измайлово",
            Description = "Мюзикл"
        };
        _eventRepoMock.Setup(x => x.FirstAsync(It.IsAny<ISpecification<Event>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(evt);
        _s3Mock.Setup(x => x.GenerateSignedUploadUrl(It.IsAny<string>())).ReturnsAsync("https://signed");

        // Act
        var result = await _eventService.GetEventUploadUrl(3, 1, Role.User);

        // Assert
        Assert.IsTrue(result.UploadUrl.StartsWith("https://signed"));
        Assert.IsTrue(result.Key.Contains("events/3/"));
    }

    [TestMethod]
    public async Task GetCreatedPrivateEventsAsync_ReturnsEvents()
    {
        // Arrange
        _eventRepoMock.Setup(x => x.ListAsync(It.IsAny<ISpecification<Event>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new Event
                {
                    Id = 1,
                    Title = "Икар",
                    Location = "КЗ Измайлово",
                    Description = "Мюзикл"
                }
            ]);

        // Act
        var result = await _eventService.GetCreatedPrivateEventsAsync(1, null, null, 0, 10);

        // Assert
        Assert.AreEqual(1, result.Count());
    }

    [TestMethod]
    public async Task GetAllCategoriesAsync_ReturnsCategories()
    {
        // Arrange
        _categoryRepoMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Category { Id = 1, Name = "rock" }]);

        // Act
        var result = await _eventService.GetAllCategoriesAsync();

        // Assert
        var categoryResponses = result.ToList();
        Assert.AreEqual(1, categoryResponses.Count);
        Assert.AreEqual("rock", categoryResponses.First().Name);
    }
}