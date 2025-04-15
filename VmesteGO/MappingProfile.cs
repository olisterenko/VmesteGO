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

        CreateMap<FriendRequest, FriendRequestResponse>()
            .ForMember(friendRequestResponse => friendRequestResponse.SenderUsername, expression => expression.MapFrom(src => src.Sender.Username))
            .ForMember(friendRequestResponse => friendRequestResponse.ReceiverUsername, expression => expression.MapFrom(src => src.Receiver.Username));
        
        
        CreateMap<Event, EventResponse>()
            .ForMember(dest => dest.CreatorUsername, opt => opt.MapFrom(src => src.Creator != null ? src.Creator.Username : "External"))
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.EventCategories.Select(ec => ec.Category.Name)))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.EventImages.Select(ei => ei.ImageKey)));

        CreateMap<CreateEventRequest, Event>(); // TODO: починить мапперы
        CreateMap<UpdateEventRequest, Event>();
        
        
        CreateMap<EventInvitation, InvitationResponse>()
            .ForMember(dest => dest.EventTitle, opt => opt.MapFrom(src => src.Event.Title))
            .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Sender.Username)) // Assuming User has Username
            .ForMember(dest => dest.ReceiverName, opt => opt.MapFrom(src => src.Receiver.Username));

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
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Event.EventCategories.Select(c => c.Category.Name)))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Event.EventImages.Select(i => i.ImageKey)))
            .ForMember(dest => dest.EventStatus, opt => opt.MapFrom(src => src.EventStatus));
    }
}