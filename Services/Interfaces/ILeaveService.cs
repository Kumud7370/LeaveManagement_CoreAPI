using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Leave;
using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Services.Interfaces
{
    public interface ILeaveService
    {
        Task<LeaveResponseDto?> CreateLeaveAsync(CreateLeaveDto dto, string createdBy);
        Task<LeaveResponseDto?> GetLeaveByIdAsync(string id);
        Task<PagedResultDto<LeaveResponseDto>> GetFilteredLeavesAsync(LeaveFilterDto filter);
        Task<PagedResultDto<LeaveResponseDto>> GetMyLeavesAsync(LeaveFilterDto filter, string userEmail);
        Task<List<LeaveResponseDto>> GetLeavesByEmployeeIdAsync(string employeeId);
        Task<List<LeaveResponseDto>> GetPendingLeavesAsync();
        Task<List<LeaveResponseDto>> GetUpcomingLeavesAsync(int days = 7);
        Task<LeaveResponseDto?> UpdateLeaveAsync(string id, UpdateLeaveDto dto, string updatedBy);
        Task<PagedResultDto<LeaveResponseDto>> GetDepartmentLeavesAsync(LeaveFilterDto filter);
        Task<bool> DeleteLeaveAsync(string id, string deletedBy);
        Task<bool> AdminApproveLeaveAsync(string id, string adminUserId);
        Task<bool> NayabApproveLeaveAsync(string id, string nayabUserId);
        Task<bool> TehsildarApproveLeaveAsync(string id, string tehsildarUserId);
        Task<bool> RejectLeaveAsync(string id, string rejectedBy, string rejectionReason);
        Task<bool> CancelLeaveAsync(string id, string cancelledBy, string cancellationReason);
        Task<Dictionary<string, int>> GetLeaveStatisticsByStatusAsync();
        Task<int> GetRemainingLeaveDaysAsync(string employeeId, string leaveTypeId, int year);
        Task<bool> ValidateLeaveRequestAsync(string employeeId, string leaveTypeId, DateTime startDate, DateTime endDate, string? excludeLeaveId = null);
        
    }
}