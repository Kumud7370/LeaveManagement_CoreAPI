using AttendanceManagementSystem.Models.DTOs.Common;
using AttendanceManagementSystem.Models.DTOs.Employee;

namespace AttendanceManagementSystem.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<EmployeeResponseDto?> CreateEmployeeAsync(CreateEmployeeDto dto, string createdBy);
        Task<EmployeeResponseDto?> GetEmployeeByIdAsync(string id);
        Task<EmployeeResponseDto?> GetEmployeeByCodeAsync(string employeeCode);
        Task<EmployeeResponseDto?> GetEmployeeByEmailAsync(string email);
        Task<PagedResultDto<EmployeeResponseDto>> GetFilteredEmployeesAsync(EmployeeFilterDto filter);
        Task<List<EmployeeResponseDto>> GetEmployeesByDepartmentAsync(string departmentId);
        Task<List<EmployeeResponseDto>> GetEmployeesByManagerAsync(string managerId);
        Task<List<EmployeeResponseDto>> GetActiveEmployeesAsync();
        Task<EmployeeResponseDto?> UpdateEmployeeAsync(string id, UpdateEmployeeDto dto, string updatedBy);
        Task<bool> DeleteEmployeeAsync(string id, string deletedBy);
        Task<bool> ChangeEmployeeStatusAsync(string id, Models.Enums.EmployeeStatus status, string updatedBy);
        Task<Dictionary<string, int>> GetEmployeeStatisticsByStatusAsync();
        Task<EmployeeResponseDto?> ReassignEmployeeAsync(string id, ReassignEmployeeDto dto, string changedBy);
        Task<List<AssignmentHistoryResponseDto>> GetAssignmentHistoryAsync(string employeeId);
        Task<BulkReassignResultDto> BulkReassignEmployeesAsync(BulkReassignEmployeeDto dto, string changedBy);
    }
}