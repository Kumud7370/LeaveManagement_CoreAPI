using AttendanceManagementSystem.Models.DTOs.LeaveBalance;
using AttendanceManagementSystem.Models.Entities;

namespace AttendanceManagementSystem.Repositories.Interfaces
{
    public interface ILeaveBalanceRepository : IBaseRepository<LeaveBalance>
    {
        Task<LeaveBalance?> GetByEmployeeAndLeaveTypeAsync(string employeeId, string leaveTypeId, int year);
        Task<List<LeaveBalance>> GetByEmployeeIdAsync(string employeeId, int? year = null);
        Task<List<LeaveBalance>> GetByLeaveTypeIdAsync(string leaveTypeId, int? year = null);
        Task<List<LeaveBalance>> GetByYearAsync(int year);
        Task<(List<LeaveBalance> Items, int TotalCount)> GetFilteredLeaveBalancesAsync(LeaveBalanceFilterDto filter);
        Task<bool> ExistsAsync(string employeeId, string leaveTypeId, int year, string? excludeId = null);
        Task<List<LeaveBalance>> GetLowBalanceAlerts(decimal threshold = 2);
        Task<List<LeaveBalance>> GetExpiringSoonAsync(int year, int daysThreshold = 30);
        Task<decimal> GetTotalConsumedByEmployeeAsync(string employeeId, int year);
        Task<Dictionary<string, decimal>> GetLeaveTypeConsumptionByEmployeeAsync(string employeeId, int year);
    }
}