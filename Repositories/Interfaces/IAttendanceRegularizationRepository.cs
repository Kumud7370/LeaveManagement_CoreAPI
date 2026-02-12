using AttendanceManagementSystem.Models.DTOs.AttendanceRegularization;
using AttendanceManagementSystem.Models.Entities;
using AttendanceManagementSystem.Models.Enums;

namespace AttendanceManagementSystem.Repositories.Interfaces
{
    public interface IAttendanceRegularizationRepository
    {
        Task<AttendanceRegularization> CreateAsync(AttendanceRegularization regularization);
        Task<bool> UpdateAsync(string id, AttendanceRegularization regularization);
        Task<bool> DeleteAsync(string id);
        Task<AttendanceRegularization?> GetByIdAsync(string id);
        Task<List<AttendanceRegularization>> GetByEmployeeIdAsync(string employeeId);
        Task<(List<AttendanceRegularization> Items, int TotalCount)> GetFilteredAsync(RegularizationFilterDto filter);
        Task<int> GetPendingCountByEmployeeAsync(string employeeId);
        Task<AttendanceRegularization?> GetByEmployeeAndDateAsync(string employeeId, DateTime date);
        Task<List<AttendanceRegularization>> GetPendingRegularizationsAsync();
        Task<List<AttendanceRegularization>> GetByStatusAsync(RegularizationStatus status);
    }
}