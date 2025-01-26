using Ardalis.Specification;
using AutoMapper;
using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Requests;
using VmesteGO.Dto.Responses;
using VmesteGO.Services.Interfaces;
using VmesteGO.Specifications.CategorySpecs;
using VmesteGO.Specifications.EventSpecs;

namespace VmesteGO.Services;

public class EventService : IEventService
{
    private readonly IRepository<Event> _eventRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IMapper _mapper;

    public EventService(
        IRepository<Event> eventRepository,
        IRepository<User> userRepository,
        IRepository<Category> categoryRepository,
        IMapper mapper)
    {
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }
    
    public async Task<EventResponse> GetEventByIdAsync(int id)
    {
        var spec = new EventsByIdSpec(id);
        var evt = await _eventRepository.FirstAsync(spec);
        return _mapper.Map<EventResponse>(evt);
    }

    public async Task<IEnumerable<EventResponse>> GetAllEventsAsync(bool includePrivate = false)
    {
        Specification<Event> spec;
        if (includePrivate)
        {
            spec = new AllEventsSpec();
        }
        else
        {
            spec = new PublicEventsSpec();
        }

        var events = await _eventRepository.ListAsync(spec);
        return _mapper.Map<IEnumerable<EventResponse>>(events);
    }
    
    public async Task<EventResponse> CreateEventAsync(CreateEventRequest createDto, int creatorId, Role role)
    {
        if (role != Role.Admin)
        {
            createDto.IsPrivate = true; 
        }

        var evt = _mapper.Map<Event>(createDto);
        evt.CreatorId = creatorId;

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
            foreach (var imageUrl in createDto.EventImages)
            {
                evt.EventImages.Add(new EventImage { ImageUrl = imageUrl, Event = evt });
            }
        }

        _eventRepository.Add(evt);
        await _eventRepository.SaveChangesAsync();

        return _mapper.Map<EventResponse>(evt);
    }
    
    public async Task<EventResponse> UpdateEventAsync(int id, UpdateEventRequest updateDto, int userId, Role role)
    {
        var evt = await _eventRepository.GetByIdAsync(id);
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
            foreach (var imageUrl in updateDto.EventImages)
            {
                evt.EventImages.Add(new EventImage { ImageUrl = imageUrl, Event = evt });
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
}