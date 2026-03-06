using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.EmployeeShift;

namespace AttendanceManagementSystem.Services.Interfaces
{
    public interface IEmployeeShiftService
    {
        // ── Admin-side ──
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

        // ── Employee self-service ──

        /// <summary>Gets all shift assignments for the currently logged-in employee (by userId).</summary>
        Task<List<EmployeeShiftResponseDto>> GetMyShiftsAsync(string userId);

        /// <summary>Employee confirms the shift assigned to them.</summary>
        Task<bool> EmployeeApproveShiftAsync(string id, string userId);

        /// <summary>Employee rejects the shift assigned to them (reason required).</summary>
        Task<bool> EmployeeRejectShiftAsync(string id, string userId, string rejectionReason);
    }
}