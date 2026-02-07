using AttendanceManagementSystem.Models.Entities;

namespace AttendanceManagementSystem.Repositories.Interfaces
{
    public interface IAdminInvitationRepository : IBaseRepository<AdminInvitation>
    {
        Task<AdminInvitation?> GetByTokenAsync(string token);
        Task<AdminInvitation?> GetByEmailAsync(string email);
        Task<IEnumerable<AdminInvitation>> GetPendingInvitationsAsync();
        Task<IEnumerable<AdminInvitation>> GetByStatusAsync(string status);
        Task<IEnumerable<AdminInvitation>> GetByInviterAsync(string inviterId);
    }
}