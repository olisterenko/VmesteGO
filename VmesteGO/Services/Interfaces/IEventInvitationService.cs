using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Requests;
using VmesteGO.Dto.Responses;

namespace VmesteGO.Services.Interfaces;

public interface IEventInvitationService
{
    Task InviteUserAsync(int eventId, int receiverId, int senderId, CancellationToken cancellationToken = default);
    Task<List<InvitationResponse>> GetInvitationsForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task RespondToInvitationAsync(int invitationId, EventInvitationStatus status, int userId, CancellationToken cancellationToken = default);
    // TODO: mb like friend requests?
}