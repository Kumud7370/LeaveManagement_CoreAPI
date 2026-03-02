using AttendanceManagementSystem.Models.DTOs.Department;
using AttendanceManagementSystem.Models.Entities;

namespace AttendanceManagementSystem.Repositories.Interfaces
{
    public interface IDepartmentRepository : IBaseRepository<Department>
    {
        Task<Department?> GetByDepartmentCodeAsync(string departmentCode);
        Task<Department?> GetByDepartmentIdAsync(Guid departmentId);
        Task<bool> IsDepartmentCodeExistsAsync(string departmentCode, Guid? excludeId = null);
        Task<(List<Department> Items, int TotalCount)> GetFilteredDepartmentsAsync(DepartmentFilterRequestDto filter);
        Task<List<Department>> GetChildDepartmentsAsync(Guid departmentId);
        Task<List<Department>> GetRootDepartmentsAsync();
        Task<List<Department>> GetActiveDepartmentsAsync();
        Task<Department?> GetDepartmentWithDetailsAsync(Guid departmentId);
        Task<int> GetEmployeeCountByDepartmentAsync(Guid departmentId);
        Task<bool> HasChildDepartmentsAsync(Guid departmentId);
        Task<bool> HasEmployeesAsync(Guid departmentId);
        Task<List<Department>> GetDepartmentHierarchyAsync();
    }
}