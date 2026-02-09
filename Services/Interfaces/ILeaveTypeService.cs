using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.LeaveType;

namespace AttendanceManagementSystem.Services.Interfaces
{
    public interface ILeaveTypeService
    {
        Task<LeaveTypeResponseDto?> CreateLeaveTypeAsync(CreateLeaveTypeDto dto, string createdBy);
        Task<LeaveTypeResponseDto?> GetLeaveTypeByIdAsync(string id);
        Task<LeaveTypeResponseDto?> GetLeaveTypeByCodeAsync(string code);
        Task<PagedResultDto<LeaveTypeResponseDto>> GetFilteredLeaveTypesAsync(LeaveTypeFilterDto filter);
        Task<List<LeaveTypeResponseDto>> GetActiveLeaveTypesAsync();
        Task<LeaveTypeResponseDto?> UpdateLeaveTypeAsync(string id, UpdateLeaveTypeDto dto, string updatedBy);
        Task<bool> DeleteLeaveTypeAsync(string id, string deletedBy);
        Task<bool> ToggleLeaveTypeStatusAsync(string id, bool isActive, string updatedBy);
    }
}
