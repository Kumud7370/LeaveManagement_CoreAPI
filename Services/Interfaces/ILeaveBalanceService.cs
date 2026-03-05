using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.LeaveBalance;

namespace AttendanceManagementSystem.Services.Interfaces
{
    public interface ILeaveBalanceService
    {
        Task<LeaveBalanceResponseDto?> CreateLeaveBalanceAsync(CreateLeaveBalanceDto dto, string createdBy);
        Task<LeaveBalanceResponseDto?> GetLeaveBalanceByIdAsync(string id);
        Task<LeaveBalanceResponseDto?> GetByEmployeeAndLeaveTypeAsync(string employeeId, string leaveTypeId, int year);
        Task<PagedResultDto<LeaveBalanceResponseDto>> GetFilteredLeaveBalancesAsync(LeaveBalanceFilterDto filter);
        Task<List<LeaveBalanceResponseDto>> GetByEmployeeIdAsync(string employeeId, int? year = null);
        Task<EmployeeLeaveBalanceSummaryDto?> GetEmployeeBalanceSummaryAsync(string employeeId, int year);
        Task<LeaveBalanceResponseDto?> UpdateLeaveBalanceAsync(string id, UpdateLeaveBalanceDto dto, string updatedBy);
        Task<bool> DeleteLeaveBalanceAsync(string id, string deletedBy);
        Task<bool> AdjustLeaveBalanceAsync(string id, AdjustLeaveBalanceDto dto, string adjustedBy);
        Task<bool> ConsumeLeaveAsync(string employeeId, string leaveTypeId, int year, decimal days);
        Task<bool> RestoreLeaveAsync(string employeeId, string leaveTypeId, int year, decimal days);
        Task<bool> CarryForwardLeaveAsync(CarryForwardDto dto, string performedBy);
        Task<List<LeaveBalanceResponseDto>> GetLowBalanceAlertsAsync(decimal threshold = 2);
        Task<List<LeaveBalanceResponseDto>> GetExpiringSoonAsync(int year, int daysThreshold = 30);
        Task<bool> InitializeBalanceForEmployeeAsync(string employeeId, int year, string createdBy);
        Task<Dictionary<string, int>> BulkInitializeBalancesAsync(BulkInitializeBalanceDto dto, string createdBy);
        Task<bool> RecalculateBalanceAsync(string id);
        Task<CollectiveAssignmentResultDto> AssignCollectiveLeaveBalanceAsync(CollectiveLeaveBalancedto dto, string createdBy);
    }
}