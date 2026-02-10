using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.EmployeeShift;

namespace AttendanceManagementSystem.Services.Interfaces
{
    public interface IEmployeeShiftService
    {
        Task<EmployeeShiftResponseDto?> CreateEmployeeShiftAsync(CreateEmployeeShiftDto dto, string createdBy);
        Task<EmployeeShiftResponseDto?> GetEmployeeShiftByIdAsync(string id);
        Task<PagedResultDto<EmployeeShiftResponseDto>> GetFilteredEmployeeShiftsAsync(EmployeeShiftFilterDto filter);
        Task<List<EmployeeShiftResponseDto>> GetEmployeeShiftsByEmployeeIdAsync(string employeeId);
        Task<EmployeeShiftResponseDto?> GetCurrentShiftForEmployeeAsync(string employeeId);
        Task<List<EmployeeShiftResponseDto>> GetPendingShiftChangesAsync();
        Task<List<EmployeeShiftResponseDto>> GetUpcomingShiftChangesAsync(int days = 7);
        Task<EmployeeShiftResponseDto?> UpdateEmployeeShiftAsync(string id, UpdateEmployeeShiftDto dto, string updatedBy);
        Task<bool> DeleteEmployeeShiftAsync(string id, string deletedBy);
        Task<bool> ApproveShiftChangeAsync(string id, string approvedBy);
        Task<bool> RejectShiftChangeAsync(string id, string rejectedBy, string rejectionReason);
        Task<bool> CancelShiftChangeAsync(string id, string updatedBy);
        Task<Dictionary<string, int>> GetShiftChangeStatisticsByStatusAsync();
        Task<bool> ValidateShiftAssignmentAsync(string employeeId, DateTime effectiveFrom, DateTime? effectiveTo, string? excludeId = null);
    }
}