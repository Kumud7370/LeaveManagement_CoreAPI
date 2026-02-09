using AttendanceManagementSystem.Models.DTOs.Leave;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Repositories.Interfaces
{
    public interface ILeaveRepository : IBaseRepository<Leave>
    {
        Task<(List<Leave> Items, int TotalCount)> GetFilteredLeavesAsync(LeaveFilterDto filter);
        Task<List<Leave>> GetLeavesByEmployeeIdAsync(string employeeId);
        Task<List<Leave>> GetLeavesByLeaveTypeIdAsync(string leaveTypeId);
        Task<List<Leave>> GetPendingLeavesAsync();
        Task<List<Leave>> GetLeavesByStatusAsync(LeaveStatus status);
        Task<List<Leave>> GetLeavesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<Leave>> GetEmployeeLeavesInRangeAsync(string employeeId, DateTime startDate, DateTime endDate);
        Task<bool> HasOverlappingLeaveAsync(string employeeId, DateTime startDate, DateTime endDate, string? excludeLeaveId = null);
        Task<int> GetApprovedLeaveDaysForEmployeeInYearAsync(string employeeId, string leaveTypeId, int year);
        Task<List<Leave>> GetUpcomingLeavesAsync(int days = 7);
    }
}
