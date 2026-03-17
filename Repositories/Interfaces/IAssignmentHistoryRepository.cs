using AttendanceManagementSystem.Models.Entities;

namespace AttendanceManagementSystem.Repositories.Interfaces
{
    public interface IAssignmentHistoryRepository
    {
        Task<EmployeeAssignmentHistory> CreateAsync(EmployeeAssignmentHistory entity);
        Task<List<EmployeeAssignmentHistory>> GetByEmployeeIdAsync(string employeeId);
        Task<EmployeeAssignmentHistory?> GetByIdAsync(string id);
    }
}
