using AttendanceManagementSystem.Models.Entities;

namespace AttendanceManagementSystem.Repositories.Interfaces
{
    public interface IAuditLogRepository : IBaseRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId);
        Task<IEnumerable<AuditLog>> GetByEntityAsync(string entity);
        Task<IEnumerable<AuditLog>> GetByActionAsync(string action);
        Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
