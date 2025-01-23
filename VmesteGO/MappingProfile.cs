using AutoMapper;
using VmesteGO.Domain.Entities;
using VmesteGO.Dto.Responses;

namespace VmesteGO;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Friend, FriendResponse>()
            .ForMember(dest => dest.FriendUsername, opt => opt.MapFrom(src => src.FriendUser.Username))
            .ForMember(dest => dest.FriendImageUrl, opt => opt.MapFrom(src => src.FriendUser.ImageUrl))
            .ForMember(dest => dest.FriendUserId, opt => opt.MapFrom(src => src.FriendUserId));

        CreateMap<FriendRequest, FriendRequestResponse>()
            .ForMember(friendRequestResponse => friendRequestResponse.SenderUsername, expression => expression.MapFrom(src => src.Sender.Username))
            .ForMember(friendRequestResponse => friendRequestResponse.ReceiverUsername, expression => expression.MapFrom(src => src.Receiver.Username));
    }
}