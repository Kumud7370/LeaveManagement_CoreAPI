using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Repositories.Implementations
{
    public class AdminInvitationRepository : BaseRepository<AdminInvitation>, IAdminInvitationRepository
    {
        public AdminInvitationRepository(IMongoDbContext context) : base(context)
        {
        }

        public async Task<AdminInvitation?> GetByTokenAsync(string token)
        {
            return await _collection.Find(x => x.Token == token && !x.IsDeleted).FirstOrDefaultAsync();
        }

        public async Task<AdminInvitation?> GetByEmailAsync(string email)
        {
            return await _collection.Find(x => x.Email == email && !x.IsDeleted).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AdminInvitation>> GetPendingInvitationsAsync()
        {
            return await _collection.Find(x => x.Status == "Pending" && !x.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<AdminInvitation>> GetByStatusAsync(string status)
        {
            return await _collection.Find(x => x.Status == status && !x.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<AdminInvitation>> GetByInviterAsync(string inviterId)
        {
            return await _collection.Find(x => x.InvitedBy == inviterId && !x.IsDeleted).ToListAsync();
        }
    }
}