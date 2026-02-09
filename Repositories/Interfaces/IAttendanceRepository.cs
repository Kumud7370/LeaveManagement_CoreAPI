using AttendanceManagementSystem.Models.DTOs.Attendance;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Repositories.Interfaces
{
    public interface IAttendanceRepository : IBaseRepository<Attendance>
    {
        Task<Attendance?> GetByEmployeeAndDateAsync(string employeeId, DateTime date);
        Task<List<Attendance>> GetByEmployeeIdAsync(string employeeId, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<Attendance>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<(List<Attendance> Items, int TotalCount)> GetFilteredAttendanceAsync(AttendanceFilterDto filter);
        Task<bool> HasCheckedInTodayAsync(string employeeId, DateTime date);
        Task<bool> HasCheckedOutTodayAsync(string employeeId, DateTime date);
        Task<List<Attendance>> GetLateCheckInsAsync(DateTime startDate, DateTime endDate);
        Task<List<Attendance>> GetEarlyLeavesAsync(DateTime startDate, DateTime endDate);
        Task<List<Attendance>> GetByDepartmentAsync(string departmentId, DateTime startDate, DateTime endDate);
        Task<int> GetAttendanceCountByStatusAsync(AttendanceStatus status, DateTime startDate, DateTime endDate);
        Task<Dictionary<AttendanceStatus, int>> GetAttendanceStatisticsAsync(DateTime startDate, DateTime endDate);
        Task<List<Attendance>> GetAbsentEmployeesAsync(DateTime date, List<string> allEmployeeIds);
    }
}