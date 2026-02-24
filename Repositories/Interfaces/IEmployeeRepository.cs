using AttendanceManagementSystem.Models.DTOs.Employee;
using AttendanceManagementSystem.Models.Entities;

namespace AttendanceManagementSystem.Repositories.Interfaces
{
    public interface IEmployeeRepository : IBaseRepository<Employee>
    {
        Task<Employee?> GetByEmployeeCodeAsync(string employeeCode);
        Task<Employee?> GetByEmailAsync(string email);
        Task<Employee?> GetByUserIdAsync(string userId); // ← ADDED
        Task<bool> IsEmployeeCodeExistsAsync(string employeeCode, string? excludeId = null);
        Task<bool> IsEmailExistsAsync(string email, string? excludeId = null);
        Task<(List<Employee> Items, int TotalCount)> GetFilteredEmployeesAsync(EmployeeFilterDto filter);
        Task<List<Employee>> GetEmployeesByDepartmentAsync(string departmentId);
        Task<List<Employee>> GetEmployeesByManagerAsync(string managerId);
        Task<List<Employee>> GetActiveEmployeesAsync();
        Task<int> GetEmployeeCountByStatusAsync(Models.Enums.EmployeeStatus status);
    }
}