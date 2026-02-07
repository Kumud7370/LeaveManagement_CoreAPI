using AttendanceManagementSystem.Models.DTOs.AdminManagement;

namespace AttendanceManagementSystem.Services.Interfaces
{
    public interface IAdminManagementService
    {
        Task<InvitationResponseDto?> SendInvitationAsync(SendInvitationDto dto, string inviterId, string inviterName, string? ipAddress = null);
        Task<InvitationResponseDto?> UpdateInvitationAsync(string invitationId, EditInvitationDto dto, string updatedBy, string? ipAddress = null);
        Task<bool> RevokeInvitationAsync(string invitationId, string revokedBy, string? ipAddress = null);
        Task<bool> DeleteInvitationAsync(string invitationId, string deletedBy, string? ipAddress = null);
        Task<IEnumerable<InvitationResponseDto>> GetAllInvitationsAsync();
        Task<IEnumerable<InvitationResponseDto>> GetMyInvitationsAsync(string inviterId);
        Task<InvitationResponseDto?> ValidateTokenAsync(string token);
        Task<bool> AcceptInvitationAsync(AcceptInvitationDto dto, string? ipAddress = null);
    }
}