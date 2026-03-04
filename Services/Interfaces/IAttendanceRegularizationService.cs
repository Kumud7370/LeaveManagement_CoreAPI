using AttendanceManagementSystem.Models.DTOs.AttendanceRegularization;
using AttendanceManagementSystem.Models.DTOs.Common;

namespace AttendanceManagementSystem.Services.Interfaces
{
    public interface IAttendanceRegularizationService
    {
        Task<RegularizationResponseDto?> RequestRegularizationAsync(RegularizationRequestDto dto, string requestedBy);
        Task<RegularizationResponseDto?> ApproveRegularizationAsync(string id, RegularizationApprovalDto dto, string approvedBy);
        Task<RegularizationResponseDto?> GetByIdAsync(string id);
        Task<List<RegularizationResponseDto>> GetByEmployeeIdAsync(string employeeId);
        Task<PagedResultDto<RegularizationResponseDto>> GetFilteredAsync(RegularizationFilterDto filter);
        Task<List<RegularizationResponseDto>> GetPendingRegularizationsAsync();
        Task<bool> CancelRegularizationAsync(string id, string cancelledBy);

        // ✅ NEW: hard soft-delete — sets IsDeleted = true via repository
        Task<bool> DeleteRegularizationAsync(string id, string deletedBy);

        Task<int> GetPendingCountByEmployeeAsync(string employeeId);
    }
}