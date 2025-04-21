using AutoMapper;
using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Requests;
using VmesteGO.Dto.Responses;

namespace VmesteGO;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<FriendRequest, FriendResponse>()
            .ForMember(dest => dest.FriendUsername, opt => opt.MapFrom(src => src.Receiver.Username))
            .ForMember(dest => dest.FriendImageUrl, opt => opt.MapFrom(src => src.Receiver.ImageKey))
            .ForMember(dest => dest.FriendUserId, opt => opt.MapFrom(src => src.ReceiverId));

        CreateMap<CreateEventRequest, Event>()
            .ForMember(dest => dest.EventCategories, opt => opt.Ignore())
            .ForMember(dest => dest.EventImages, opt => opt.Ignore());
        CreateMap<UpdateEventRequest, Event>()
            .ForMember(dest => dest.EventCategories, opt => opt.Ignore())
            .ForMember(dest => dest.EventImages, opt => opt.Ignore());
        
        CreateMap<CreateInvitationRequest, EventInvitation>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => EventInvitationStatus.Pending));

        CreateMap<RespondInvitationResponse, EventInvitation>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
        
        
        CreateMap<UserRegisterRequest, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.Salt, opt => opt.Ignore())
            .ForMember(dest => dest.SentFriendRequests, opt => opt.Ignore())
            .ForMember(dest => dest.ReceivedFriendRequests, opt => opt.Ignore())
            .ForMember(dest => dest.SentEventInvitations, opt => opt.Ignore())
            .ForMember(dest => dest.ReceivedEventInvitations, opt => opt.Ignore())
            .ForMember(dest => dest.Notifications, opt => opt.Ignore())
            .ForMember(dest => dest.Comments, opt => opt.Ignore())
            .ForMember(dest => dest.UserEvents, opt => opt.Ignore());

        CreateMap<UserUpdateRequest, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<User, UserResponse>();
        
        CreateMap<Notification, NotificationResponse>();
        
        CreateMap<UserEvent, EventResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Event.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Event.Title))
            .ForMember(dest => dest.Dates, opt => opt.MapFrom(src => src.Event.Dates))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Event.Location))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Event.Description))
            .ForMember(dest => dest.AgeRestriction, opt => opt.MapFrom(src => src.Event.AgeRestriction))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Event.Price))
            .ForMember(dest => dest.IsPrivate, opt => opt.MapFrom(src => src.Event.IsPrivate))
            .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.Event.ExternalId))
            .ForMember(dest => dest.CreatorId, opt => opt.MapFrom(src => src.Event.CreatorId))
            .ForMember(dest => dest.CreatorUsername, opt => opt.MapFrom(src => src.Event.Creator != null ? src.Event.Creator.Username : string.Empty))
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Event.EventCategories.Select(c => c.Category)))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Event.EventImages.Select(i => i.ImageKey)))
            .ForMember(dest => dest.EventStatus, opt => opt.MapFrom(src => src.EventStatus));
    }
}