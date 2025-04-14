using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Responses;

namespace VmesteGO.Services.Interfaces;

public interface IEventInvitationService
{
    Task InviteUserAsync(int eventId, int receiverId, int senderId, CancellationToken cancellationToken = default);

    Task<List<InvitationResponse>> GetPendingEventInvitationsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<List<InvitationResponse>> GetSentEventInvitationsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task RespondToInvitationAsync(
        int invitationId,
        EventInvitationStatus status,
        int receiverId,
        CancellationToken cancellationToken = default);

    Task RevokeInvitationAsync(int invitationId, int senderId, CancellationToken cancellationToken);
}