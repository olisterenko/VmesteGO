using AutoMapper;
using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Requests;
using VmesteGO.Dto.Responses;
using VmesteGO.Services.Interfaces;
using VmesteGO.Specifications.CategorySpecs;
using VmesteGO.Specifications.EventSpecs;
using VmesteGO.Specifications.UserEventSpecs;

namespace VmesteGO.Services;

public class EventService : IEventService
{
    private readonly IRepository<Event> _eventRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<UserEvent> _userEventRepository;
    private readonly IMapper _mapper;
    private readonly IS3StorageService _s3Service;
    private readonly IRepository<EventImage> _eventImageRepository;


    public EventService(
        IRepository<Event> eventRepository,
        IRepository<User> userRepository,
        IRepository<Category> categoryRepository,
        IRepository<UserEvent> userEventRepository,
        IMapper mapper, IS3StorageService s3Service, 
        IRepository<EventImage> eventImageRepository)
    {
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _s3Service = s3Service;
        _eventImageRepository = eventImageRepository;
        _userEventRepository = userEventRepository;
    }
    
    public async Task<EventResponse> GetEventByIdAsync(int id)
    {
        var spec = new EventsByIdSpec(id);
        var evt = await _eventRepository.FirstAsync(spec);
        return _mapper.Map<EventResponse>(evt);
    }

    public async Task<IEnumerable<EventResponse>> GetEventsAsync(GetEventsRequest getEventsRequest)
    {
        if (getEventsRequest is { UserId: not null, EventStatus: not null })
        {
            var userEvents = await _userEventRepository
                .ListAsync(new EventsByEventStatusSpec(getEventsRequest.UserId.Value, getEventsRequest.EventStatus.Value));
            
            return userEvents.Select(evt => _mapper.Map<EventResponse>(evt));
        }

        var events =  await _eventRepository.ListAsync(new AllEventsSpec());
        return events.Select(evt => _mapper.Map<EventResponse>(evt));
    }
    
    public async Task<EventResponse> CreateEventAsync(CreateEventRequest createDto, int creatorId, Role role)
    {
        if (role != Role.Admin)
        {
            createDto.IsPrivate = true; 
        }

        var evt = _mapper.Map<Event>(createDto);
        evt.CreatorId = creatorId; // TODO: Creator Entity

        if (createDto.EventCategoryIds.Count != 0)
        {
            var categoriesSpec = new CategoriesByIdsSpec(createDto.EventCategoryIds);
            var categories = await _categoryRepository.ListAsync(categoriesSpec);
            foreach (var category in categories)
            {
                evt.EventCategories.Add(new EventCategory { CategoryId = category.Id, Event = evt });
            }
        }

        if (createDto.EventImages.Count != 0)
        {
            foreach (var imageKey in createDto.EventImages)
            {
                evt.EventImages.Add(new EventImage { ImageKey = imageKey, Event = evt });
            }
        }

        _eventRepository.Add(evt);
        await _eventRepository.SaveChangesAsync();

        return _mapper.Map<EventResponse>(evt);
    }
    
    public async Task<EventResponse> UpdateEventAsync(int id, UpdateEventRequest updateDto, int userId, Role role)
    {
        var evt = await _eventRepository.GetByIdAsync(id); // TODO: spec for categories and images
        if (evt == null)
        {
            throw new KeyNotFoundException($"Event with id {id} not found."); // TODO: NotFoundException and filter
        }

        if (role != Role.Admin && evt.CreatorId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to update this event.");
        }

        _mapper.Map(updateDto, evt);

        evt.EventCategories.Clear();
        if (updateDto.EventCategoryIds.Count != 0)
        {
            var categoriesSpec = new CategoriesByIdsSpec(updateDto.EventCategoryIds);
            var categories = await _categoryRepository.ListAsync(categoriesSpec);
            foreach (var category in categories)
            {
                evt.EventCategories.Add(new EventCategory { CategoryId = category.Id, Event = evt });
            }
        }

        evt.EventImages.Clear();
        if (updateDto.EventImages.Count != 0)
        {
            foreach (var imageKey in updateDto.EventImages)
            {
                evt.EventImages.Add(new EventImage { ImageKey = imageKey, Event = evt });
            }
        }
        
        
        await _eventRepository.SaveChangesAsync();

        return _mapper.Map<EventResponse>(evt);
    }
    
    public async Task DeleteEventAsync(int id, int userId, Role role)
    {
        var evt = await _eventRepository.GetByIdAsync(id);
        if (evt == null)
        {
            throw new KeyNotFoundException($"Event with id {id} not found.");
        }

        if (role != Role.Admin && evt.CreatorId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this event.");
        }

        _eventRepository.Delete(evt);
        await _eventRepository.SaveChangesAsync();
    }

    public async Task ChangeEventStatus(ChangeEventStatusRequest changeEventStatusRequest)
    {
        var userEvent = await _userEventRepository
            .FirstOrDefaultAsync(new UserEventByUserAndEventSpec(changeEventStatusRequest.UserId, changeEventStatusRequest.EventId));

        if (userEvent is null && changeEventStatusRequest.NewEventStatus != EventStatus.NotGoing)
        {
            userEvent = new UserEvent
            {
                EventId = changeEventStatusRequest.EventId,
                UserId = changeEventStatusRequest.UserId,
                EventStatus = changeEventStatusRequest.NewEventStatus
            };

            await _userEventRepository.AddAsync(userEvent);

            return;
        }

        if (userEvent is null)
        {
            return;
        }

        if (changeEventStatusRequest.NewEventStatus == EventStatus.NotGoing)
        {
            await _userEventRepository.DeleteAsync(userEvent);

            return;
        }

        userEvent.EventStatus = changeEventStatusRequest.NewEventStatus;
        await _userEventRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<EventResponse>> GetCreatedPrivateEventsAsync(int userId, string q, int offset, int limit)
    {
        var events = await _eventRepository.ListAsync(new CreatedPrivateEventsSpecification(userId, q, offset, limit));
        return _mapper.Map<IEnumerable<EventResponse>>(events);
    }

    public async Task<IEnumerable<EventResponse>> GetJoinedPrivateEventsAsync(int userId, string q, int offset, int limit)
    {
        var events = await _eventRepository.ListAsync(new JoinedPrivateEventsSpecification(userId, q, offset, limit));
        return _mapper.Map<IEnumerable<EventResponse>>(events);
    }

    public async Task<IEnumerable<EventResponse>> GetCreatedPublicEventsAsync(int userId, string q, int offset, int limit)
    {
        var events = await _eventRepository.ListAsync(new CreatedPublicEventsSpecification(userId, q, offset, limit));
        return _mapper.Map<IEnumerable<EventResponse>>(events);
    }

    public async Task<IEnumerable<EventResponse>> GetOtherAdminsPublicEventsAsync(int userId, string q, int offset, int limit)
    {
        var events = await _eventRepository.ListAsync(new OtherAdminsPublicEventsSpecification(userId, q, offset, limit));
        return _mapper.Map<IEnumerable<EventResponse>>(events);
    }

    public async Task<UploadEventImageUrlResponse> GetEventUploadUrl(int id, int userId, Role role)
    {
        var spec = new EventsByIdSpec(id);
        var evt = await _eventRepository.FirstAsync(spec);
        
        if (evt == null)
        {
            throw new KeyNotFoundException($"Event with id {id} not found."); // TODO: NotFoundException and filter
        }

        if (role != Role.Admin && evt.CreatorId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to update this event.");
        }
        
        var imageId = Guid.NewGuid().ToString();
        var key = $"events/{id}/{imageId}.jpg";

        var url = await _s3Service.GenerateSignedUploadUrl(key);
        
        return new UploadEventImageUrlResponse(url, key, evt.EventImages.Count + 1);
    }

    public async Task SaveImageMetadataAsync(int eventId, string imageKey, int orderIndex, int userId, Role role)
    {
        var spec = new EventsByIdSpec(eventId);
        var evt = await _eventRepository.FirstAsync(spec);
        
        if (evt == null)
        {
            throw new KeyNotFoundException($"Event with id {eventId} not found."); // TODO: NotFoundException and filter
        }

        if (role != Role.Admin && evt.CreatorId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to update this event.");
        }
        
        var newImage = new EventImage
        {
            EventId = eventId,
            ImageKey = imageKey,
            OrderIndex = orderIndex
        };

        await _eventImageRepository.AddAsync(newImage);
    }

    public async Task DeleteImageAsync(int imageId)
    {
        var image = await _eventImageRepository.GetByIdAsync(imageId);

        await _s3Service.DeleteImageAsync(image.ImageKey);
        await _eventImageRepository.DeleteAsync(image);
        await _eventImageRepository.SaveChangesAsync();
        await ReorderImagesAsync(image.EventId);
    }
    
    private async Task ReorderImagesAsync(int eventId)
    {
        var spec = new EventsByIdSpec(eventId);
        var evt = await _eventRepository.FirstAsync(spec);
        var images = evt.EventImages
            .OrderBy(e => e.OrderIndex)
            .ToList();

        for (var i = 0; i < images.Count; i++)
        {
            images[i].OrderIndex = i + 1;
        }

        await _eventRepository.SaveChangesAsync();
    }
}