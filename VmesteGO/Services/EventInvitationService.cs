using AutoMapper;
using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Responses;
using VmesteGO.Services.Interfaces;
using VmesteGO.Specifications.EventInvitationSpecs;
using VmesteGO.Specifications.UserEventSpecs;

namespace VmesteGO.Services;

public class EventInvitationService : IEventInvitationService
{
    private readonly IRepository<Event> _eventRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<EventInvitation> _invitationRepository;
    private readonly IRepository<UserEvent> _userEventRepository;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public EventInvitationService(
        IRepository<Event> eventRepository,
        IRepository<User> userRepository,
        IRepository<EventInvitation> invitationRepository,
        IRepository<UserEvent> userEventRepository,
        INotificationService notificationService,
        IMapper mapper)
    {
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _invitationRepository = invitationRepository;
        _userEventRepository = userEventRepository;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task InviteUserAsync(int eventId, int receiverId, int senderId,
        CancellationToken cancellationToken = default)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(eventId, cancellationToken);
        var sender = await _userRepository.GetByIdAsync(senderId, cancellationToken);

        await _userRepository.GetByIdAsync(receiverId, cancellationToken);

        var existingInvitationSpec = new EventInvitationByEventAndReceiverSpec(eventId, receiverId);
        var existingInvitation =
            await _invitationRepository.FirstOrDefaultAsync(existingInvitationSpec, cancellationToken);
        if (existingInvitation != null)
            throw new InvalidOperationException("An invitation has already been sent to this user for this event");

        var userEventSpec = new UserEventByUserAndEventSpec(receiverId, eventId);
        var userEvent = await _userEventRepository.FirstOrDefaultAsync(userEventSpec, cancellationToken);
        if (userEvent != null)
            throw new InvalidOperationException("User is already part of the event");

        var invitation = new EventInvitation
        {
            EventId = eventId,
            SenderId = senderId,
            ReceiverId = receiverId,
            Status = EventInvitationStatus.Pending,
        };

        await _invitationRepository.AddAsync(invitation, cancellationToken);
        await _invitationRepository.SaveChangesAsync(cancellationToken);

        await _notificationService.AddNotificationAsync(
            receiverId,
            $"You were invited to the event \"{eventEntity.Title}\" by {sender.Username}.",
            cancellationToken
        );
    }

    public async Task<List<InvitationResponse>> GetPendingEventInvitationsAsync(int userId,
        CancellationToken cancellationToken = default)
    {
        var spec = new ReceivedEventInvitationsSpec(userId);
        var invitations = await _invitationRepository.ListAsync(spec, cancellationToken);

        return _mapper.Map<List<InvitationResponse>>(invitations);
    }

    public async Task<List<InvitationResponse>> GetSentEventInvitationsAsync(int userId,
        CancellationToken cancellationToken = default)
    {
        var spec = new SentEventInvitationsSpec(userId);
        var invitations = await _invitationRepository.ListAsync(spec, cancellationToken);

        return _mapper.Map<List<InvitationResponse>>(invitations);
    }

    public async Task RespondToInvitationAsync(int invitationId, EventInvitationStatus status, int receiverId,
        CancellationToken cancellationToken = default)
    {
        var spec = new EventInvitationWithUsersAndEvent(invitationId);
        var invitation = await _invitationRepository.FirstAsync(spec, cancellationToken);

        if (invitation.ReceiverId != receiverId)
            throw new UnauthorizedAccessException("You are not authorized to respond to this invitation");

        if (status != EventInvitationStatus.Accepted && status != EventInvitationStatus.Rejected)
            throw new InvalidOperationException("Invalid status");

        invitation.Status = status;

        if (status == EventInvitationStatus.Accepted)
        {
            var userEventSpec = new UserEventByUserAndEventSpec(receiverId, invitation.EventId);
            var userEvent = await _userEventRepository.FirstOrDefaultAsync(userEventSpec, cancellationToken);
            if (userEvent == null)
            {
                var userEventEntity = new UserEvent
                {
                    UserId = receiverId,
                    EventId = invitation.EventId,
                    EventStatus = EventStatus.Going
                };
                _userEventRepository.Add(userEventEntity);
            }
        }

        await _invitationRepository.SaveChangesAsync(cancellationToken);
        await _userEventRepository.SaveChangesAsync(cancellationToken);

        await _notificationService.AddNotificationAsync(
            invitation.SenderId,
            $"You got an answer from {invitation.Receiver.Username} to your invitation to \"{invitation.Event.Title}\". The status is {invitation.Status}",
            cancellationToken
        );
    }

    public async Task RevokeInvitationAsync(int invitationId, int senderId, CancellationToken cancellationToken)
    {
        var spec = new EventInvitationWithUsersAndEvent(invitationId);
        var invitation = await _invitationRepository.FirstAsync(spec, cancellationToken);

        if (invitation.SenderId != senderId)
            throw new UnauthorizedAccessException("You are not authorized to revoke to this invitation");

        await _invitationRepository.DeleteAsync(invitation, cancellationToken);
        
        await _notificationService.AddNotificationAsync(
            invitation.ReceiverId,
            $"Invitation to \"{invitation.Event.Title}\" by {invitation.Sender.Username} was revoked.",
            cancellationToken
        );
    }
}