using AttendanceManagementSystem.Models.DTOs.Designation;
using AttendanceManagementSystem.Models.Entities;

namespace AttendanceManagementSystem.Repositories.Interfaces
{
    public interface IDesignationRepository : IBaseRepository<Designation>
    {
        Task<Designation?> GetByDesignationCodeAsync(string designationCode);
        Task<bool> IsDesignationCodeExistsAsync(string designationCode, string? excludeId = null);
        Task<(List<Designation> Items, int TotalCount)> GetFilteredDesignationsAsync(DesignationFilterDto filter);
        Task<List<Designation>> GetByLevelAsync(int level);
        Task<List<Designation>> GetActiveDesignationsAsync();
        Task<int> GetEmployeeCountByDesignationAsync(string designationId);
        Task<List<Designation>> GetByDepartmentIdAsync(string departmentId);
    }
}