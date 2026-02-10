using AttendanceManagementSystem.Models.DTOs.EmployeeShift;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Repositories.Interfaces
{
    public interface IEmployeeShiftRepository : IBaseRepository<EmployeeShift>
    {
        Task<(List<EmployeeShift> Items, int TotalCount)> GetFilteredEmployeeShiftsAsync(EmployeeShiftFilterDto filter);
        Task<List<EmployeeShift>> GetByEmployeeIdAsync(string employeeId);
        Task<EmployeeShift?> GetCurrentShiftForEmployeeAsync(string employeeId);
        Task<List<EmployeeShift>> GetByShiftIdAsync(string shiftId);
        Task<List<EmployeeShift>> GetPendingShiftChangesAsync();
        Task<List<EmployeeShift>> GetByStatusAsync(ShiftChangeStatus status);
        Task<bool> HasActiveShiftAsync(string employeeId, DateTime effectiveDate, string? excludeId = null);
        Task<bool> HasOverlappingShiftAssignmentAsync(string employeeId, DateTime effectiveFrom, DateTime? effectiveTo, string? excludeId = null);
        Task<int> GetPendingShiftChangesCountForEmployeeAsync(string employeeId);
        Task<List<EmployeeShift>> GetUpcomingShiftChangesAsync(int days = 7);
    }
}