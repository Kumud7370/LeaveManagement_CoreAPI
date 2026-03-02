using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.WorkFromHome;
using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Services.Interfaces
{
    public interface IWorkFromHomeRequestService
    {
        Task<WfhRequestResponseDto?> CreateWfhRequestAsync(string employeeId, CreateWfhRequestDto dto, string createdBy);
        Task<WfhRequestResponseDto?> GetWfhRequestByIdAsync(string id);
        Task<PagedResultDto<WfhRequestResponseDto>> GetFilteredWfhRequestsAsync(WfhRequestFilterDto filter);
        Task<List<WfhRequestResponseDto>> GetEmployeeWfhRequestsAsync(string employeeId);
        Task<List<WfhRequestResponseDto>> GetPendingWfhRequestsAsync();
        Task<List<WfhRequestResponseDto>> GetActiveWfhRequestsAsync();
        Task<WfhRequestResponseDto?> UpdateWfhRequestAsync(string id, UpdateWfhRequestDto dto, string updatedBy);
        Task<bool> DeleteWfhRequestAsync(string id, string deletedBy);
        Task<WfhRequestResponseDto?> ApproveRejectWfhRequestAsync(string id, ApproveRejectWfhRequestDto dto, string approverId);
        Task<bool> CancelWfhRequestAsync(string id, string cancelledBy);
        Task<Dictionary<string, int>> GetWfhRequestStatisticsByStatusAsync();
        Task<List<WfhRequestResponseDto>> GetEmployeeWfhRequestsByDateRangeAsync(string employeeId, DateTime startDate, DateTime endDate);
        Task<WfhRequestResponseDto?> CreateWfhRequestByUserAsync(string userId, string userEmail, CreateWfhRequestDto dto);
        Task<List<WfhRequestResponseDto>> GetMyWfhRequestsByUserAsync(string userId, string? userEmail);
    }
}