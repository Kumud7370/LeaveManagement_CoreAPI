using AttendanceManagementSystem.Models.DTOs.LeaveType;
using AttendanceManagementSystem.Models.Entities;

namespace AttendanceManagementSystem.Repositories.Interfaces
{
    public interface ILeaveTypeRepository : IBaseRepository<LeaveType>
    {
        Task<LeaveType?> GetByCodeAsync(string code);
        Task<bool> IsCodeExistsAsync(string code, string? excludeId = null);
        Task<(List<LeaveType> Items, int TotalCount)> GetFilteredLeaveTypesAsync(LeaveTypeFilterDto filter);
        Task<List<LeaveType>> GetActiveLeaveTypesAsync();
        Task<List<LeaveType>> GetLeaveTypesRequiringApprovalAsync();
        Task<List<LeaveType>> GetLeaveTypesRequiringDocumentAsync();
        Task<int> GetMaxDisplayOrderAsync();
    }
}
