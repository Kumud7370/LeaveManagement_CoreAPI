using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Repositories.Interfaces;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Repositories.Implementations
{
    public class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(IMongoDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId)
        {
            return await _collection.Find(x => x.UserId == userId && !x.IsDeleted)
                .SortByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entity)
        {
            return await _collection.Find(x => x.Entity == entity && !x.IsDeleted)
                .SortByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action)
        {
            return await _collection.Find(x => x.Action == action && !x.IsDeleted)
                .SortByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _collection.Find(x => x.Timestamp >= startDate && x.Timestamp <= endDate && !x.IsDeleted)
                .SortByDescending(x => x.Timestamp)
                .ToListAsync();
        }
    }
}