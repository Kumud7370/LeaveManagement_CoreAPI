using AttendanceManagementSystem.Models.DTOs.WorkFromHome;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Repositories.Interfaces
{
    public interface IWorkFromHomeRequestRepository : IBaseRepository<WorkFromHomeRequest>
    {
        Task<(List<WorkFromHomeRequest> Items, int TotalCount)> GetFilteredWfhRequestsAsync(WfhRequestFilterDto filter);
        Task<List<WorkFromHomeRequest>> GetByEmployeeIdAsync(string employeeId);
        Task<List<WorkFromHomeRequest>> GetPendingRequestsAsync();
        Task<List<WorkFromHomeRequest>> GetApprovedRequestsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<bool> HasOverlappingRequestAsync(string employeeId, DateTime startDate, DateTime endDate, string? excludeId = null);
        Task<List<WorkFromHomeRequest>> GetActiveWfhRequestsAsync();
        Task<int> GetWfhRequestCountByStatusAsync(ApprovalStatus status);
        Task<List<WorkFromHomeRequest>> GetEmployeeWfhRequestsByDateRangeAsync(string employeeId, DateTime startDate, DateTime endDate);
    }
}